using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.VSPackage.WebServices;
using OSBIDE.Library.Models;
using OSBIDE.Library.Events;
using OSBIDE.Library;
using System.Data.SqlServerCe;
using System.Threading;
using System.Runtime.Caching;

namespace OSBIDE.VSPackage
{
    public class ServiceClient
    {
        private OsbideWebServiceClient _webServiceClient = null;
        private OsbideUser _currentUser;
        private EventHandlerBase _events;
        private List<EventLog> _pendingLogs = new List<EventLog>();
        private ILogger _logger;
        private ObjectCache _cache = new FileCache(StringConstants.LocalCacheDirectory, new LibraryBinder());
        private string _cacheRegion = "ServiceClient";
        private string _cacheKey = "logs";

        public bool HasWebServiceError { get; set; }

        private void SaveToCache(List<EventLog> logs)
        {
            _cache.Set(_cacheKey, logs.ToArray(), new CacheItemPolicy(), _cacheRegion);
        }

        private List<EventLog> GetFromCache()
        {
            return ((EventLog[])_cache.Get(_cacheKey, _cacheRegion)).ToList();
        }

        public ServiceClient(EventHandlerBase dteEventHandler, OsbideUser user, ILogger logger)
        {
            _events = dteEventHandler;
            _currentUser = user;
            this._logger = logger;
            
            _webServiceClient = new OsbideWebServiceClient(ServiceBindings.OsbideServiceBinding, ServiceBindings.OsbideServiceEndpoint);

            //AC: "events" ends up being null during unit testing.  Otherwise, it should never happen.
            if (_events != null)
            {
                _events.EventCreated += new EventHandler<EventCreatedArgs>(OsbideEventCreated);
            }

            //if we don't have a cache record of pending logs when we start, create a dummy list
            if (!_cache.Contains(_cacheKey, _cacheRegion))
            {
                SaveToCache(new List<EventLog>());
            }
        }

        private void SendLogToServer(object data)
        {

            //only accept eventlog data
            if (!(data is EventLog))
            {
                return;
            }

            //cast generic data to what we actually need
            EventLog newLog = data as EventLog;

            //find all logs that haven't been handled (submitted)
            List<EventLog> logsToBeSaved = null;
            
            //request exclusive access to our cache of existing logs
            lock (_cache)
            {
                //get pending records
                logsToBeSaved = GetFromCache();

                //add new log to list
                logsToBeSaved.Add(newLog);

                //clear out cache
                SaveToCache(new List<EventLog>());
            }

            //loop through each log to be saved, give a dummy ID number
            int counter = 1;
            foreach (EventLog log in logsToBeSaved)
            {
                log.Id = counter;
                counter++;
            }

            //will hold the list of saved logs
            List<int> savedLogs = new List<int>(logsToBeSaved.Count);
            
            //send logs to the server
            foreach (EventLog log in logsToBeSaved)
            {
                //reset the log's sending user just to be safe
                log.SenderId = _currentUser.Id;
                log.Sender = _currentUser;
                _logger.WriteToLog(string.Format("Sending log with ID {0} to the server", log.Id));

                try
                {
                    //the number that comes back from the web service is the log's local ID number.  Save
                    //for later when we clean up our local db.
                    int result = _webServiceClient.SubmitLog(log);
                    savedLogs.Add(result);
                }
                catch (Exception ex)
                {
                    //Log any error that we might've received.  Most likely an issue finding the endpoint (server)
                    _logger.WriteToLog(string.Format("SaveLogs error: {0}", ex.Message));

                    //turn off logging for this session so that we don't bog down the client with tons of failed
                    //service calls.
                    HasWebServiceError = true;
                }
            }

            //any logs that weren't saved successfully get added back into the cache
            foreach (int logId in savedLogs)
            {
                EventLog log = logsToBeSaved.Where(l => l.Id == logId).FirstOrDefault();
                if (log != null)
                {
                    logsToBeSaved.Remove(log);
                }
            }

            //save the modified list back into the cache
            lock (_cache)
            {
                SaveToCache(logsToBeSaved);
            }
        }


        /// <summary>
        /// Called whenever OSBIDE detects an event change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OsbideEventCreated(object sender, EventCreatedArgs e)
        {
            //create a new event log...
            EventLog eventLog = new EventLog(e.OsbideEvent, _currentUser);
            
            //if we haven't gotten a service error this session, send it off.  Otherwise,
            //save to cache and try again next session
            if (HasWebServiceError == false)
            {
                Thread serverThread = new Thread(this.SendLogToServer);
                serverThread.Start(eventLog);
            }
            else
            {
                lock (_cache)
                {
                    List<EventLog> cachedLogs = GetFromCache();
                    cachedLogs.Add(eventLog);
                    SaveToCache(cachedLogs);
                }
            }
        }
    }
}

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
using System.Data.Entity.Validation;
using System.ComponentModel;
using OSBIDE.Library.Logging;
using System.Threading.Tasks;

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
        private Task _eventLogTask;
        private string _cacheRegion = "ServiceClient";
        private string _cacheKey = "logs";
        private ServiceClientState _clientState = null;
        private int _pushErrorCounter = 0;
        private int _getErrorCounter = 0;

        public ServiceClient(EventHandlerBase dteEventHandler, OsbideUser user, ILogger logger)
        {
            _events = dteEventHandler;
            _currentUser = user;
            this._logger = logger;
            _clientState = ServiceClientState.Instance;
            _webServiceClient = new OsbideWebServiceClient(ServiceBindings.OsbideServiceBinding, ServiceBindings.OsbideServiceEndpoint);

            //AC: "events" ends up being null during unit testing.  Otherwise, it should never happen.
            if (_events != null)
            {
                _events.EventCreated += new EventHandler<EventCreatedArgs>(OsbideEventCreated);
            }

            //if we don't have a cache record of pending logs when we start, create a dummy list
            if (!_cache.Contains(_cacheKey, _cacheRegion))
            {
                SaveLogsToCache(new List<EventLog>());
            }

            _clientState.PropertyChanged += new PropertyChangedEventHandler(ClientStatePropertyChanged);

            //set up and begin event log thread
            _eventLogTask = Task.Factory.StartNew(
                () =>
                {
                    PullFromServer();
                }
                );
        }

        void ClientStatePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsPerformingWebGets")
            {
                if (_clientState.IsPerformingWebGets == true)
                {
                    _eventLogTask = Task.Factory.StartNew(
                        () =>
                        {
                            PullFromServer();
                        }
                        );
                }
            }
            else if (e.PropertyName == "IsPerformingWebPushes")
            {
                if (_clientState.IsPerformingWebPushes == true)
                {
                    _pushErrorCounter = 0;
                }
            }
        }

        private void PushToServerError(Exception ex)
        {
            _logger.WriteToLog(string.Format("Push error: {0}", ex.Message), LogPriority.HighPriority);
            _pushErrorCounter++;

            //if we've received more than 10 errors, tell everyone that we give up
            if (_pushErrorCounter > 10)
            {
                _clientState.IsPerformingWebPushes = false;
                _pushErrorCounter = 0;
            }
        }

        private void SaveLogsToCache(List<EventLog> logs)
        {
            _cache.Set(_cacheKey, logs.ToArray(), new CacheItemPolicy(), _cacheRegion);
        }

        private List<EventLog> GetLogsFromCache()
        {
            return ((EventLog[])_cache.Get(_cacheKey, _cacheRegion)).ToList();
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
                logsToBeSaved = GetLogsFromCache();

                //add new log to list
                logsToBeSaved.Add(newLog);

                //clear out cache
                SaveLogsToCache(new List<EventLog>());
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
                _logger.WriteToLog(string.Format("Sending log with ID {0} to the server", log.Id), LogPriority.LowPriority);

                try
                {
                    //the number that comes back from the web service is the log's local ID number.  Save
                    //for later when we clean up our local db.
                    int result = _webServiceClient.SubmitLog(log);
                    savedLogs.Add(result);
                    _clientState.LastWebPush = DateTime.Now;
                }
                catch (Exception ex)
                {
                    PushToServerError(ex);
                    break;
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
                SaveLogsToCache(logsToBeSaved);
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

            //if the system is allowing web pushes, send it off.  Otherwise,
            //save to cache and try again later
            if (_clientState.IsPerformingWebPushes)
            {
                Task.Factory.StartNew(
                    () =>
                    {
                        this.SendLogToServer(eventLog);
                    }
                    );
            }
            else
            {
                lock (_cache)
                {
                    List<EventLog> cachedLogs = GetLogsFromCache();
                    cachedLogs.Add(eventLog);
                    SaveLogsToCache(cachedLogs);
                }
            }
        }

        private void PullFromServer()
        {
            try
            {
                EventsFromServerLoop();
            }
            catch (TimeoutException tex)
            {
                _clientState.IsPerformingWebGets = false;
                _logger.WriteToLog("EventsFromServerLoop timeout exception: " + tex.Message, LogPriority.HighPriority);
            }
            catch (DbEntityValidationException ex)
            {
                _clientState.IsPerformingWebGets = false;
                foreach (DbEntityValidationResult result in ex.EntityValidationErrors)
                {
                    foreach (DbValidationError error in result.ValidationErrors)
                    {
                        _logger.WriteToLog("EventsFromServerLoop insert log exception: " + error.ErrorMessage, LogPriority.HighPriority);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.WriteToLog("EventsFromServerLoop exception: " + ex.Message, LogPriority.HighPriority);
                _clientState.IsPerformingWebGets = false;
            }
        }

        private void EventsFromServerLoop()
        {
            //start with a really long time ago
            DateTime startDate = DateTime.Now.Subtract(new TimeSpan(365, 0, 0, 0, 0));

            SqlCeConnection conn = null;
            OsbideContext db = null;

            //make sure we have SQL Server CE installed before we continue
            if (OsbideContext.HasSqlServerCE)
            {
                conn = new SqlCeConnection(StringConstants.LocalDataConnectionString);
                db = new OsbideContext(conn, true);
            }
            else
            {
                throw new Exception("Error connecting to SQL Server CE database");
            }

            while (_clientState.IsPerformingWebGets)
            {
                //use most recent event log as a basis for determining where we need to fill in potential gaps in data
                EventLog mostRecentLog = (from log in db.EventLogs
                                          orderby log.DateReceived descending
                                          select log).FirstOrDefault();

                if (mostRecentLog != null)
                {
                    startDate = mostRecentLog.DateReceived;
                }

                EventLog[] logs = _webServiceClient.GetPastEvents(startDate, true);
                _clientState.LastWebPull = DateTime.Now;

                //find all unique User Ids
                List<int> eventLogIds = new List<int>();
                foreach (EventLog log in logs)
                {
                    int index = eventLogIds.BinarySearch(log.SenderId);
                    if (index < 0)
                    {
                        //from http://msdn.microsoft.com/en-us/library/w4e7fxsh.aspx:
                        //taking the bitwise NOT of the index returns the first element that
                        //is larger than the supplied index, thereby inserting the current
                        //element in the right place.
                        eventLogIds.Insert(~index, log.SenderId);
                    }
                }

                //see if we're missing any ids in our local DB
                var eventLogQuery = from id in eventLogIds
                                    select id;
                var idQuery = from user in db.Users
                              where eventLogIds.Contains(user.Id)
                              select user.Id;
                int[] foundIds = idQuery.ToArray();

                foreach (int id in foundIds)
                {
                    eventLogIds.Remove(id);
                }

                //if we have any remaining ids, then we need to make the appropriate db call
                if (eventLogIds.Count > 0)
                {
                    OsbideUser[] missingUsers = _webServiceClient.GetUsers(eventLogIds.ToArray());
                    foreach (OsbideUser user in missingUsers)
                    {
                        db.InsertUserWithId(user);
                    }
                }

                //finally, insert the event logs
                foreach (EventLog log in logs)
                {
                    bool successfulInsert = db.InsertEventLogWithId(log);

                    if (successfulInsert && log.LogType == SubmitEvent.Name)
                    {
                        SubmitEvent submit = (SubmitEvent)EventFactory.FromZippedBinary(log.Data, new OsbideDeserializationBinder());
                        submit.EventLogId = log.Id;
                        db.SubmitEvents.Add(submit);
                        db.SaveChanges();
                    }

                }
            }

            db.Dispose();
        }
    }
}

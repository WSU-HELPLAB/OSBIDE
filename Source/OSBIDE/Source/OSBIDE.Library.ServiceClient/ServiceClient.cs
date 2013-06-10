using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using OSBIDE.Library.ServiceClient.OsbideWebService;
using System.IO;

namespace OSBIDE.Library.ServiceClient
{
    public class ServiceClient : INotifyPropertyChanged
    {
        private static ServiceClient _instance;

        #region instance variables
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private OsbideWebServiceClient _webServiceClient = null;
        private EventHandlerBase _events;
        private List<EventLog> _pendingLogs = new List<EventLog>();
        private ILogger _logger;
        private ObjectCache _cache = new FileCache(StringConstants.LocalCacheDirectory, new LibraryBinder());
        private Task _eventLogTask;
        private Task _sendLocalErrorsTask;
        private Task _checkKeyTask;
        private string _cacheRegion = "ServiceClient";
        private string _cacheKey = "logs";
        private bool _isSendingData = true;
        private bool _isReceivingData = true;
        private TransmissionStatus _sendStatus = new TransmissionStatus();
        private TransmissionStatus _receiveStatus = new TransmissionStatus();
        private bool _canCheckKey = true;

        #endregion

        #region properties

        public bool IsSendingData
        {
            get
            {
                lock (this)
                {
                    return _isSendingData;
                }
            }
            private set
            {
                lock (this)
                {
                    _isSendingData = value;
                }
                OnPropertyChanged("IsSendingData");
            }
        }
        public bool IsReceivingData
        {
            get
            {
                lock (this)
                {
                    return _isReceivingData;
                }
            }
            private set
            {
                lock (this)
                {
                    _isReceivingData = value;
                }
                OnPropertyChanged("IsSendingData");
            }
        }
        public TransmissionStatus SendStatus
        {
            get
            {
                lock (this)
                {
                    return _sendStatus;
                }
            }
        }
        public TransmissionStatus ReceiveStatus
        {
            get
            {
                lock (this)
                {
                    return _receiveStatus;
                }
            }
        }
        #endregion

        #region constructor

        private ServiceClient(EventHandlerBase dteEventHandler, ILogger logger)
        {
            _events = dteEventHandler;
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
                SaveLogsToCache(new List<EventLog>());
            }
            
            //send off saved local errors
            _sendLocalErrorsTask = Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        SendLocalErrorLogs();
                    }
                    catch (Exception ex)
                    {
                        _logger.WriteToLog("Error sending local logs to server: " + ex.Message, LogPriority.MediumPriority);
                    }
                }
                );

            //register a thread to keep our service key from going stale
            _checkKeyTask = Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        CheckKey();
                    }
                    catch (Exception ex)
                    {
                        _logger.WriteToLog("Error in CheckKey: " + ex.Message, LogPriority.MediumPriority);
                    }
                }
                );

            //set up and begin event log thread
            //(turned off for fall study)
            /*
            _eventLogTask = Task.Factory.StartNew(
                () =>
                {
                    PullFromServer();
                }
                );
             * */
        }

        #endregion

        #region public methods

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Returns a singleton instance of <see cref="ServiceClient"/>.  Unlike a normal
        /// singleton pattern, this will return NULL if GetInstance(EventHandlerBase dteEventHandler, ILogger logger)
        /// was not called first.
        /// </summary>
        /// <returns></returns>
        public static ServiceClient GetInstance()
        {
            return _instance;
        }

        /// <summary>
        /// Returns a singleton instance of <see cref="ServiceClient"/>.  Parameters are only 
        /// used during the first instantiation of the <see cref="ServiceClient"/>.
        /// </summary>
        /// <param name="dteEventHandler"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static ServiceClient GetInstance(EventHandlerBase dteEventHandler, ILogger logger)
        {
            if (_instance == null)
            {
                _instance = new ServiceClient(dteEventHandler, logger);
            }
            return _instance;
        }

        public void TurnOnReceiving()
        {
            IsReceivingData = true;

            //only start a new thread if the existing one has finished
            if (_eventLogTask.IsCompleted == true)
            {
                //turned off for fall study
                /*
                _eventLogTask = Task.Factory.StartNew(
                () =>
                {
                    PullFromServer();
                }
                );
                 * */
            }
        }

        public void TurnOnSending()
        {
            IsSendingData = true;
        }

        #endregion

        #region private send methods

        private void CheckKey()
        {
            while (_canCheckKey == true)
            {
                lock (_cache)
                {
                    string webServiceKey = _cache[StringConstants.AuthenticationCacheKey] as string;
                    bool result = _webServiceClient.IsValidKey(webServiceKey);

                    //if result is false, our key has gone stale.  Try to login again
                    if (result == false)
                    {
                        string userName = _cache[StringConstants.UserNameCacheKey] as string;
                        string password = _cache[StringConstants.PasswordCacheKey] as string;
                        if (userName != null && password != null)
                        {
                            webServiceKey = _webServiceClient.Login(userName, UserPassword.EncryptPassword(password, userName));
                            _cache[StringConstants.AuthenticationCacheKey] = webServiceKey;
                        }
                        else
                        {
                            IsSendingData = false;
                        }
                    }
                }
                Thread.Sleep(new TimeSpan(0, 0, 3, 0, 0));
            }
        }

        private void SendLocalErrorLogs()
        {
            string dataRoot = StringConstants.DataRoot;
            string logExtension = StringConstants.LocalErrorLogExtension;
            string today = StringConstants.LocalErrorLogFileName;

            //find all log files
            string[] files = Directory.GetFiles(dataRoot);
            foreach (string file in files)
            {
                if (Path.GetExtension(file) == logExtension)
                {
                    //ignore today's log
                    if (Path.GetFileNameWithoutExtension(file) != today)
                    {
                        LocalErrorLog log = LocalErrorLog.FromFile(file);
                        int result = 0;
                        lock (_cache)
                        {
                            string webServiceKey = _cache[StringConstants.AuthenticationCacheKey] as string;
                            result = _webServiceClient.SubmitLocalErrorLog(log, webServiceKey);
                        }

                        //remove if file successfully sent
                        if (result != -1)
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }
        }

        private void SendError(Exception ex)
        {
            _logger.WriteToLog(string.Format("Push error: {0}", ex.Message), LogPriority.HighPriority);
            IsSendingData = false;
        }

        private void SendLogToServer(object data)
        {

            //only accept eventlog data
            if (!(data is EventLog))
            {
                return;
            }
            SendStatus.IsActive = true;

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

            //reorder by date received (created in our case)
            logsToBeSaved = logsToBeSaved.OrderBy(l => l.DateReceived).ToList();

            //loop through each log to be saved, give a dummy ID number
            int counter = 1;
            foreach (EventLog log in logsToBeSaved)
            {
                log.Id = counter;
                counter++;
            }

            //update our send status with the number of logs that we
            //plan to submit
            SendStatus.NumberOfTransmissions = logsToBeSaved.Count;

            //will hold the list of saved logs
            List<int> savedLogs = new List<int>(logsToBeSaved.Count);

            //send logs to the server
            foreach (EventLog log in logsToBeSaved)
            {
                //reset the log's sending user just to be safe
                _logger.WriteToLog(string.Format("Sending log with ID {0} to the server", log.Id), LogPriority.LowPriority);
                SendStatus.CurrentTransmission = log;

                try
                {
                    //the number that comes back from the web service is the log's local ID number.  Save
                    //for later when we clean up our local db.
                    int result = -1;
                    lock (_cache)
                    {
                        string webServiceKey = _cache[StringConstants.AuthenticationCacheKey] as string;
                        result = _webServiceClient.SubmitLog(log, webServiceKey);
                    }
                    savedLogs.Add(result);

                    //update our submission status
                    SendStatus.LastTransmissionTime = DateTime.UtcNow;
                    SendStatus.LastTransmission = log;
                    SendStatus.CompletedTransmissions++;
                }
                catch (Exception ex)
                {
                    SendError(ex);
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
            SendStatus.IsActive = false;
        }

        /// <summary>
        /// Called whenever OSBIDE detects an event change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OsbideEventCreated(object sender, EventCreatedArgs e)
        {
            //create a new event log...
            EventLog eventLog = new EventLog(e.OsbideEvent);
            SendStatus.IsActive = false;

            //if the system is allowing web pushes, send it off.  Otherwise,
            //save to cache and try again later
            if (IsSendingData)
            {
                Task.Factory.StartNew(
                    () =>
                    {
                        try
                        {
                            this.SendLogToServer(eventLog);
                        }
                        catch (Exception ex)
                        {
                            _logger.WriteToLog(string.Format("SendToServer Error: {0}", ex.Message), LogPriority.HighPriority);
                        }
                    }
                    );
            }
            else
            {
                SendStatus.IsActive = false;
                lock (_cache)
                {
                    List<EventLog> cachedLogs = GetLogsFromCache();
                    cachedLogs.Add(eventLog);
                    SaveLogsToCache(cachedLogs);
                }
            }
        }

        #endregion

        #region private receive methods
        private void PullFromServer()
        {
            try
            {
                ReceiveStatus.IsActive = true;

                //not used for fall release 
                //EventsFromServerLoop();
            }
            catch (TimeoutException tex)
            {
                IsReceivingData = false;
                _logger.WriteToLog("EventsFromServerLoop timeout exception: " + tex.Message, LogPriority.HighPriority);
            }
            catch (DbEntityValidationException ex)
            {
                IsReceivingData = false;
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
                IsReceivingData = false;
            }
            ReceiveStatus.IsActive = false;
        }

        private void EventsFromServerLoop()
        {
            //start with a really long time ago
            DateTime startDate = DateTime.UtcNow.Subtract(new TimeSpan(365, 0, 0, 0, 0));

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

            while (IsReceivingData)
            {
                //use most recent event log as a basis for determining where we need to fill in potential gaps in data
                EventLog mostRecentLog = (from log in db.EventLogs
                                          orderby log.DateReceived descending
                                          select log).FirstOrDefault();

                if (mostRecentLog != null)
                {
                    startDate = mostRecentLog.DateReceived;
                }
                else
                {
                    //AC Note: mostRecentLog cannot be NULL because we use it to set the last transmission
                    //time.  This can probably be simplified.
                    mostRecentLog = new EventLog();
                    mostRecentLog.DateReceived = startDate;
                }

                EventLog[] logs = new EventLog[0];// _webServiceClient.GetPastEvents(startDate, true);
                ReceiveStatus.LastTransmissionTime = mostRecentLog.DateReceived;

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
                int[] foundIds = new int[0];

                //From http://brandonzeider.me/2011/microsoft-net/entity-frameworksql-server-ce-thread-safety/
                //suggestion is to thread lock db calls to prevent memory access violations.
                lock (db)
                {
                    var idQuery = from user in db.Users
                                  where eventLogIds.Contains(user.Id)
                                  select user.Id;
                    foundIds = idQuery.ToArray();
                }

                foreach (int id in foundIds)
                {
                    eventLogIds.Remove(id);
                }
                /*
                //if we have any remaining ids, then we need to make the appropriate db call
                if (eventLogIds.Count > 0)
                {
                    OsbideUser[] missingUsers = _webServiceClient.GetUsers(eventLogIds.ToArray());
                    foreach (OsbideUser user in missingUsers)
                    {
                        lock (db)
                        {
                            db.InsertUserWithId(user);
                        }
                    }
                }*/

                //finally, insert the event logs
                foreach (EventLog log in logs)
                {
                    bool successfulInsert = db.InsertEventLogWithId(log);

                    if (successfulInsert && log.LogType == SubmitEvent.Name)
                    {
                        SubmitEvent submit = (SubmitEvent)EventFactory.FromZippedBinary(log.Data.BinaryData, new OsbideDeserializationBinder());
                        submit.EventLogId = log.Id;

                        lock (db)
                        {
                            db.SubmitEvents.Add(submit);
                            db.SaveChanges();
                        }
                    }

                }
            }

            ReceiveStatus.IsActive = false;
            db.Dispose();
        }
        #endregion

        #region private helpers
        private void SaveLogsToCache(List<EventLog> logs)
        {
            _cache.Set(_cacheKey, logs.ToArray(), new CacheItemPolicy(), _cacheRegion);
        }

        private List<EventLog> GetLogsFromCache()
        {
            List<EventLog> logs = new List<EventLog>();
            //get pending records
            try
            {
                logs = ((EventLog[])_cache.Get(_cacheKey, _cacheRegion)).ToList();
            }
            catch (Exception ex)
            {
                //saved logs corrupted, start over
                SaveLogsToCache(new List<EventLog>());
                logs = new List<EventLog>();
                _logger.WriteToLog(string.Format("GetLogsFromCache() error: {0}", ex.Message), LogPriority.HighPriority);
            }
            return logs;
        }
        #endregion
    }
}

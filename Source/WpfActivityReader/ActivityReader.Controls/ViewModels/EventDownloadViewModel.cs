using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActivityReader.Library;
using ActivityReader.Controls.OsbideService;
using OSBIDE.Library.Models;
using System.Data.SqlServerCe;
using OSBIDE.Library.Events;
using System.Windows.Input;
using System.Threading.Tasks;

namespace ActivityReader.Controls.ViewModels
{
    public class EventDownloadViewModel : ViewModelBase
    {
        private OsbideWebServiceClient _serviceClient;
        private OsbideContext _db;
        private bool _isDownloading = false;

        private string _status;
        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public ICommand DownloadCommand { get; set; }

        public EventDownloadViewModel()
        {
            _serviceClient = new OsbideWebServiceClient();
            SqlCeConnection conn = new SqlCeConnection(StringConstants.SqlCeConnectionString);
            _db = new OsbideContext(conn, true);
            DownloadCommand = new DelegateCommand(StartDownload, CanDownload);
            Status = "Not downloading";
        }

        private bool CanDownload(object param)
        {
            return !_isDownloading;
        }

        private void StartDownload(object param)
        {
            Task.Factory.StartNew(
                () =>
                {
                    _isDownloading = true;
                    DownloadTask();
                    _isDownloading = false;
                    Status = "Download complete.";
                }
                );
        }

        private void DownloadTask()
        {
            
            int downloadCounter = 0;
            EventLog[] webLogs = new EventLog[0];
            do
            {
                Status = string.Format("Downloaded {0} items", downloadCounter);
                DateTime lastEvent = (from log in _db.EventLogs
                                      orderby log.DateReceived descending
                                      select log.DateReceived).FirstOrDefault();
                if (lastEvent == null)
                {
                    lastEvent = DateTime.MinValue;
                }
                webLogs = _serviceClient.GetPastEvents(lastEvent, false);
                downloadCounter += webLogs.Length;

                //find all unique User Ids
                List<int> eventLogIds = new List<int>();
                foreach (EventLog log in webLogs)
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
                var idQuery = from user in _db.Users
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
                    OsbideUser[] missingUsers = _serviceClient.GetUsers(eventLogIds.ToArray());
                    foreach (OsbideUser user in missingUsers)
                    {
                        _db.InsertUserWithId(user);
                    }
                }

                //finally, insert the event logs
                foreach (EventLog log in webLogs)
                {
                    bool successfulInsert = _db.InsertEventLogWithId(log);

                    /*
                    if (successfulInsert && log.LogType == SubmitEvent.Name)
                    {
                        SubmitEvent submit = (SubmitEvent)EventFactory.FromZippedBinary(log.Data, new OsbideDeserializationBinder());
                        submit.EventLogId = log.Id;
                        _db.SubmitEvents.Add(submit);
                        _db.SaveChanges();
                    }
                     * */

                }
            }while (webLogs.Length != 0);
        }
    }
}

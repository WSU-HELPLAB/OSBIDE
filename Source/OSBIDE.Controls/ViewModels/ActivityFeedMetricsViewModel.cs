using OSBIDE.Controls.Models;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Controls.ViewModels
{
    public class ActivityFeedMetricsViewModel : ViewModelBase
    {
        private OsbideContext _db = null;
        public TimeSpan ActivityFeedSessionTimeout { get; set; }

        public ActivityFeedMetricsViewModel(OsbideContext db)
        {
            _db = db;
        }


        public void GetActivityFeedSessionInfo(DateTime startDate, DateTime endDate)
        {
            //A session is defined as usage that contains no more than 10 minutes of inactivity
            
            //goal: loop through all event logs:
            //  If current user doesn't have a session, create one with starting and ending times as the current log
            //  If the difference between the last observed log's date and the current log's date is greater than
            //  the timeout grade period, then start a new session.

            //step #1: get all logs within our date range
            Dictionary<int, List<ActivityFeedSession>> sessions = new Dictionary<int, List<ActivityFeedSession>>();
            var logsQuery = _db.EventLogs
                .Where(e => e.DateReceived >= startDate)
                .Where(e => e.DateReceived <= endDate)
                .OrderBy(e => e.Id);
            foreach (EventLog log in logsQuery)
            {
                //add key if it doesn't already exist
                if (sessions.ContainsKey(log.SenderId) == false)
                {
                    sessions.Add(log.SenderId, new List<ActivityFeedSession>());
                    ActivityFeedSession session = new ActivityFeedSession()
                    {
                        User = log.Sender,
                        StartDate = log.DateReceived,
                        EndDate = log.DateReceived
                    };
                }

                //check to see if the current log exceeds the session timeout

            }
        }
    }
}

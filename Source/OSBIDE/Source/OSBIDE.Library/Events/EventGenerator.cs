using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.Library.Models;

namespace OSBIDE.Library.Events
{
    public class EventGenerator : IOsbideEventGenerator
    {
        private static EventGenerator _instance = null;

        public event EventHandler<SubmitEventArgs> SolutionSubmitRequest = delegate { };
        public event EventHandler<SolutionDownloadedEventArgs> SolutionDownloaded = delegate { };

        private EventGenerator()
        {

        }

        public static EventGenerator GetInstance()
        {
            if (_instance == null)
            {
                _instance = new EventGenerator();
            }
            return _instance;
        }

        /// <summary>
        /// Triggers a request for the system to save the active solution
        /// </summary>
        public void RequestSolutionSubmit(OsbideUser requestAuthor, string assignmentName)
        {
            SolutionSubmitRequest(requestAuthor, new SubmitEventArgs(assignmentName));
        }

        public void NotifySolutionDownloaded(OsbideUser downloadingUser, SubmitEvent downloadedSubmission)
        {
            SolutionDownloaded(downloadingUser, new SolutionDownloadedEventArgs(downloadingUser, downloadedSubmission));
        }
    }

    public class SubmitEventArgs : EventArgs
    {
        public string AssignmentName { get; private set; }
        public SubmitEventArgs(string assignmentName)
        {
            AssignmentName = assignmentName;
        }
    }

    public class SolutionDownloadedEventArgs : EventArgs
    {
        public OsbideUser DownloadingUser { get; private set; }
        public SubmitEvent DownloadedSubmission { get; private set; }

        public SolutionDownloadedEventArgs(OsbideUser downloadingUser, SubmitEvent downloadedSubmission)
        {
            DownloadingUser = downloadingUser;
            DownloadedSubmission = downloadedSubmission;
        }
    }
}

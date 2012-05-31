using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class SolutionDownloadEvent : IOsbideEvent
    {
        public DateTime EventDate { get; set; }
        public string SolutionName { get; set; }
        public string AssignmentName { get; set; }
        public int DownloadingUserId { get; set; }
        public int AuthorId { get; set; }
        public string EventName { get { return SolutionDownloadEvent.Name; } }
        public static string Name { get { return "SolutionDownloadEvent"; } }
        public SolutionDownloadEvent()
        {
            EventDate = DateTime.Now;
        }
    }
}

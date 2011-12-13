using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.Library.Models;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class BuildEvent : IOsbideEvent
    {
        public DateTime EventDate { get; set; }
        public string SolutionName { get; set; }
        public IList<ErrorListItem> ErrorItems { get; set; }
        public string EventName { get { return BuildEvent.Name; } }
        public static string Name { get { return "BuildEvent"; } }
        public BuildEvent()
        {
            ErrorItems = new List<ErrorListItem>();
            EventDate = DateTime.Now;
        }

    }
}

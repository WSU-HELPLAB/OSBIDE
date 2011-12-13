using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Events
{
    public class SaveEvent : IOsbideEvent
    {
        public DateTime EventDate { get; set; }
        public string SolutionName { get; set; }
        public string DocumentContent { get; set; }
        public string DocumentName { get; set; }
        public string EventName { get { return "SaveEvent"; } }
    }
}

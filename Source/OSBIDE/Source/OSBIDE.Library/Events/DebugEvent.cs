using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class DebugEvent : IOsbideEvent
    {
        public DateTime EventDate { get; set; }
        public string SolutionName { get; set; }
        public int EventReason { get; set; }
        public int ExecutionAction { get; set; }
        public string DocumentName { get; set; }
        public string EventName
        {
            get { return "DebugEvent"; }
        }
        public DebugEvent()
        {
            EventDate = DateTime.Now;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.Library.Models;

namespace OSBIDE.Library.Events
{
    public class BreakpointToggleEvent : IOsbideEvent
    {
        public DateTime EventDate { get; set; }
        public string SolutionName { get; set; }
        public string EventName { get { return "BreakpointToggleEvent"; } }
        public BreakPoint Breakpoint { get; set; }

        public BreakpointToggleEvent()
        {
            EventDate = DateTime.Now;
        }
    }
}

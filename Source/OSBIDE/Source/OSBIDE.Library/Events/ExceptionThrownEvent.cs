using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class ExceptionEvent : IOsbideEvent
    {
        public DateTime EventDate { get; set; }
        public string SolutionName { get; set; }
        public string EventName
        {
            get { return "ExceptionThrownEvent"; }
        }
        public string ExceptionType { get; set; }
        public string ExceptionName { get; set; }
        public int ExceptionCode { get; set; }
        public string ExceptionDescription { get; set; }
        public int ExceptionAction { get; set; }
        public string DocumentName { get; set; }

        public ExceptionEvent()
        {
            EventDate = DateTime.Now;
        }
    }
}

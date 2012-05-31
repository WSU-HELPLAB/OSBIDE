using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.Library.Models;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class SaveEvent : IOsbideEvent
    {
        public DateTime EventDate { get; set; }
        public string SolutionName { get; set; }
        public string EventName { get { return SaveEvent.Name; } }
        public static string Name { get { return "SaveEvent"; } }
        public IVSDocument Document { get; set; }

        public SaveEvent()
        {
            EventDate = DateTime.Now;
        }
    }
}

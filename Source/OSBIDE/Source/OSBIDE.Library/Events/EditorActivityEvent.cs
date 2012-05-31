using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class EditorActivityEvent : IOsbideEvent
    {
        public DateTime EventDate { get; set; }
        public string SolutionName { get; set; }
        public string EventName { get { return EditorActivityEvent.Name; } }
        public static string Name { get { return "EditorActivityEvent"; } }

        public EditorActivityEvent()
        {
            EventDate = DateTime.Now;
        }
    }
}

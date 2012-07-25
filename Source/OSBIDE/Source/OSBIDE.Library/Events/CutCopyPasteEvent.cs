using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using OSBIDE.Library.Models;

namespace OSBIDE.Library.Events
{
    [Serializable]
    class CutCopyPasteEvent : IOsbideEvent
    {
        public DateTime EventDate { get; set; }
        public string SolutionName { get; set; }
        public int EventAction { get; set; }

        [NotMapped]
        public CutCopyPasteActions Action
        {
            get
            {
                return (CutCopyPasteActions)EventAction;
            }
            set
            {
                EventAction = (int)value;
            }
        }
        public string DocumentName { get; set; }
        public string Content { get; set; }

        public string EventName { get { return CutCopyPasteEvent.Name; } }
        public static string Name { get { return "CutCopyPasteEvent"; } }
        public CutCopyPasteEvent()
        {
            EventDate = DateTime.Now;
        }
    }
}

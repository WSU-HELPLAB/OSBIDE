using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using System.ComponentModel.DataAnnotations;
using OSBIDE.Library.Models;

namespace OSBIDE.Library.Events
{

    [Serializable]
    public class DebugEvent : IOsbideEvent
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int EventLogId { get; set; }

        [ForeignKey("EventLogId")]
        public virtual EventLog EventLog { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public string SolutionName { get; set; }

        [Required]
        public int ExecutionAction { get; set; }

        [Required]
        public string DocumentName { get; set; }

        /// <summary>
        /// If the event was a Step (over, into, out of), will track the line number
        /// on which the event took place.
        /// </summary>
        [Required]
        public int LineNumber { get; set; }

        /// <summary>
        /// Contains information in the debug output window.  As the output window is cumulative, there's no
        /// reason to set this unless you're dealing with a "StopDebugging" event
        /// </summary>
        [Required]
        public string DebugOutput { get; set; }

        [Required]
        public string EventName { get { return DebugEvent.Name; } }

        [NotMapped]
        public static string Name { get { return "DebugEvent"; } }


        public DebugEvent()
        {
            EventDate = DateTime.Now;
            LineNumber = -1;
        }
    }
}

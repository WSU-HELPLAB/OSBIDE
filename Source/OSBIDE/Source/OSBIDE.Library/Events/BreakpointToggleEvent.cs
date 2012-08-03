using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.Library.Models;
using System.ComponentModel.DataAnnotations;

namespace OSBIDE.Library.Events
{
    public class BreakpointToggleEvent : IOsbideEvent
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
        public string EventName { get { return "BreakpointToggleEvent"; } }

        [NotMapped]
        public BreakPoint Breakpoint { get; set; }

        public BreakpointToggleEvent()
        {
            EventDate = DateTime.Now;
        }
    }
}

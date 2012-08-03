using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.Library.Models;
using System.ComponentModel.DataAnnotations;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class BuildEvent : IOsbideEvent
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
        public IList<BuildEventErrorListItem> ErrorItems { get; set; }

        [Required]
        public IList<BuildEventBreakPoint> Breakpoints { get; set; }

        [Required]
        public string EventName { get { return BuildEvent.Name; } }

        [NotMapped]
        public static string Name { get { return "BuildEvent"; } }
        public BuildEvent()
        {
            ErrorItems = new List<BuildEventErrorListItem>();
            Breakpoints = new List<BuildEventBreakPoint>();
            EventDate = DateTime.Now;
        }

    }
}

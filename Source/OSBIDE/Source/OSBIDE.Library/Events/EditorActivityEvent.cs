using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using OSBIDE.Library.Models;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class EditorActivityEvent : IOsbideEvent
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
        public string EventName { get { return EditorActivityEvent.Name; } }

        [NotMapped]
        public static string Name { get { return "EditorActivityEvent"; } }

        public EditorActivityEvent()
        {
            EventDate = DateTime.Now;
        }
    }
}

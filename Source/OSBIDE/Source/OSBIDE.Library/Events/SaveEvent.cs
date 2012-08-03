using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.Library.Models;
using System.ComponentModel.DataAnnotations;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class SaveEvent : IOsbideEvent
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
        public string EventName { get { return SaveEvent.Name; } }

        [NotMapped]
        public static string Name { get { return "SaveEvent"; } }

        [Required]
        public IVSDocument Document { get; set; }

        public SaveEvent()
        {
            EventDate = DateTime.Now;
        }
    }
}

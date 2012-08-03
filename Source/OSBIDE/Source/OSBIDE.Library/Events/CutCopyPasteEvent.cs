using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using OSBIDE.Library.Models;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class CutCopyPasteEvent : IOsbideEvent
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

        [Required]
        public string DocumentName { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string EventName { get { return CutCopyPasteEvent.Name; } }

        [NotMapped]
        public static string Name { get { return "CutCopyPasteEvent"; } }

        public CutCopyPasteEvent()
        {
            EventDate = DateTime.Now;
        }
    }
}

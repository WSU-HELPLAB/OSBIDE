using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using OSBIDE.Library.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class SolutionDownloadEvent : IOsbideEvent
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

        [Required(AllowEmptyStrings = true)]
        public string SolutionName { get; set; }

        [Required]
        public string AssignmentName { get; set; }

        [Required]
        public int DownloadingUserId { get; set; }

        [Required]
        public int AuthorId { get; set; }
        
        [Required]
        public string EventName { get { return SolutionDownloadEvent.Name; } }

        [NotMapped]
        public static string Name { get { return "SolutionDownloadEvent"; } }

        [NotMapped]
        public string PrettyName { get { return "Solution Download"; } }

        public SolutionDownloadEvent()
        {
            EventDate = DateTime.Now;
        }
    }
}

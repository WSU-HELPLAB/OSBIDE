using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.Library.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class ExceptionEvent : IOsbideEvent
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
        public string EventName { get { return ExceptionEvent.Name; } }

        [NotMapped]
        public static string Name { get { return "ExceptionEvent"; } }

        [NotMapped]
        public string PrettyName { get { return "Exception"; } }

        [Required]
        public string ExceptionType { get; set; }

        [Required]
        public string ExceptionName { get; set; }

        [Required]
        public int ExceptionCode { get; set; }

        [Required]
        public string ExceptionDescription { get; set; }

        [Required]
        public int ExceptionAction { get; set; }

        [Required]
        public string DocumentName { get; set; }

        [Required]
        public int LineNumber { get; set; }

        [Required]
        public string LineContent { get; set; }

        public virtual IList<StackFrame> StackFrames { get; set; }

        public ExceptionEvent()
        {
            EventDate = DateTime.Now;
            StackFrames = new List<StackFrame>();
        }
    }
}

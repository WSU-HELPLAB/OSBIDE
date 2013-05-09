using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class AskForHelpEvent : IOsbideEvent
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
        public string EventName { get { return AskForHelpEvent.Name; } }

        [NotMapped]
        public static string Name { get { return "AskForHelpEvent"; } }

        [NotMapped]
        public string PrettyName { get { return "Ask for Help"; } }

        [Required]
        public string Code { get; set; }

        [Required]
        public string UserComment { get; set; }

        public string GetShortComment(int maxLength)
        {
            if (UserComment.Length < maxLength)
            {
                return UserComment;
            }
            while (UserComment[maxLength] != ' ' && maxLength >= 0)
            {
                maxLength--;
            }
            return UserComment.Substring(0, maxLength);
        }
    }
}

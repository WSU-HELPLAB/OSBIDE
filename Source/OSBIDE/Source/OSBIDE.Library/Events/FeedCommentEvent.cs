﻿using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class FeedCommentEvent : IOsbideEvent
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
        public string EventName { get { return FeedCommentEvent.Name; } }

        [NotMapped]
        public static string Name { get { return "FeedCommentEvent"; } }

        [NotMapped]
        public string PrettyName { get { return "Feed Post"; } }

        [Required]
        public string Comment { get; set; }

        public FeedCommentEvent()
        {
            EventDate = DateTime.UtcNow;
            SolutionName = "";
        }

        public string GetShortComment(int maxLength)
        {
            if (Comment.Length < maxLength)
            {
                return Comment;
            }
            while(Comment[maxLength] != ' ' && maxLength >= 0)
            {
                maxLength--;
            }
            return Comment.Substring(0, maxLength);
        }
    }
}

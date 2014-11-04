using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Terminal.ViewModels
{
    public enum CommentType { FeedPost, LogComment}
    public class Comment
    {
        public int UserId { get; set; }
        public int StudentId { get; set; }
        public int CommentId { get; set; }
        public CommentType CommentType { get; set; }
        public string Content { get; set; }
        public int WordCount { get; set; }
        public int SentenceCount { get; set; }
        public double AverageSyllablesPerWord { get; set; }
        public double FleschReadingEase { get; set; }
        public double FleschKincaidGradeLevel { get; set; } 
    }
}

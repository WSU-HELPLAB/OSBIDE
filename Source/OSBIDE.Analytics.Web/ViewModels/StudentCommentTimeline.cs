using OSBIDE.Analytics.Library.Models;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Analytics.Web.ViewModels
{
    public class StudentCommentTimeline
    {
        public List<TimelineState> TimelineStates { get; set; }
        public List<PostCoding> CrowdCodings { get; set; }
        public ContentCoding ExpertCoding { get; set; }
        public string Comment { get; set; }
        public EventLog Log { get; set; }
        public Dictionary<int, CodeDocument> CodeBeforeComment { get; set; }
        public Dictionary<int, CodeDocument> CodeAfterComment { get; set; }
        public OsbideUser Author { get; set; }
        public StudentCommentTimeline()
        {
            TimelineStates = new List<TimelineState>();
            CrowdCodings = new List<PostCoding>();
            CodeBeforeComment = new Dictionary<int, CodeDocument>();
            CodeAfterComment = new Dictionary<int, CodeDocument>();
        }
    }
}
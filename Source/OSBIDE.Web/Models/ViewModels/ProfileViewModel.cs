using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OSBIDE.Library.Models;
using OSBIDE.Library.Events;

namespace OSBIDE.Web.Models.ViewModels
{
    public class ProfileViewModel
    {
        public OsbideUser User { get; set; }
        public FeedViewModel Feed { get; set; }
        public List<LogCommentEvent> RecentComments { get; set; }
        public List<LogCommentEvent> CommentsMadeByOthers { get; set; }
        public List<AggregateFeedItem> EventLogSubscriptions { get; set; }
        public UserScore Score { get; set; }
        public int NumberOfComments { get; set; }
        public int NumberOfPosts { get; set; }
        public ProfileViewModel()
        {
            Feed = new FeedViewModel();
            RecentComments = new List<LogCommentEvent>();
            EventLogSubscriptions = new List<AggregateFeedItem>();
        }
    }
}
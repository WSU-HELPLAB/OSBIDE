using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OSBIDE.Library.Models;

namespace OSBIDE.Web.Models.ViewModels
{
    public class ProfileViewModel
    {
        public OsbideUser User { get; set; }
        public FeedViewModel Feed { get; set; }
        public List<LogComment> RecentComments { get; set; }
        public List<AggregateFeedItem> EventLogSubscriptions { get; set; }

        public ProfileViewModel()
        {
            Feed = new FeedViewModel();
            RecentComments = new List<LogComment>();
            EventLogSubscriptions = new List<AggregateFeedItem>();
        }
    }
}
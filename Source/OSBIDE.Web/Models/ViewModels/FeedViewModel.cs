using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models.ViewModels
{
    public class FeedViewModel
    {
        public List<AggregateFeedItem> Feed { get; set; }
        public DateTime LastPollDate { get; set; }
        public int LastLogId { get; set; }
        public int SingleUserId { get; set; }
        public List<IOsbideEvent> EventFilterOptions { get; set; }
        public List<IOsbideEvent> UserEventFilterOptions { get; set; }
        public List<ErrorType> ErrorTypes { get; set; }
        public ErrorType SelectedErrorType { get; set; }
        public List<string> RecentUserErrors { get; set; }
        public List<UserBuildErrorsByType> RecentClassErrors { get; set; }

        public FeedViewModel()
        {
            Feed = new List<AggregateFeedItem>();
            SingleUserId = -1;
            LastLogId = -1;
            LastPollDate = DateTime.UtcNow;
            EventFilterOptions = new List<IOsbideEvent>();
            UserEventFilterOptions = new List<IOsbideEvent>();
            ErrorTypes = new List<ErrorType>();
            SelectedErrorType = new ErrorType();
            RecentUserErrors = new List<string>();
            RecentClassErrors = new List<UserBuildErrorsByType>();
        }
    }
}
using DiffMatchPatch;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using OSBIDE.Web.Models;
using OSBIDE.Web.Models.Attributes;
using OSBIDE.Web.Models.Queries;
using OSBIDE.Web.Models.ViewModels;
using OSBIDE.Web.Services;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    [OsbideAuthorize]
    [RequiresVisualStudioConnectionForStudents]
    public class FeedController : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">The ID of the last event received by the user.  Used for AJAX updates</param>
        /// <returns></returns>
        public ActionResult Index(long timestamp = -1, int errorType = -1)
        {
            string[] errors = base.GetRecentCompileErrors(CurrentUser);
            ActivityFeedQuery query = BuildBasicQuery();
            FeedViewModel vm = new FeedViewModel();

            if (timestamp > 0)
            {
                DateTime pullDate = new DateTime(timestamp);
                query.StartDate = pullDate;
            }
            else
            {
                query.StartDate = DateTime.Now.AddHours(-10);
                
            }
            
            //and finally, retrieve our list of feed items
            var maxIdQuery = Db.EventLogs.Select(l => l.Id);
            if (maxIdQuery.Count() > 0)
            {
                vm.LastLogId = maxIdQuery.Max();
            }
            else
            {
                vm.LastLogId = 0;
            }
            List<FeedItem> feedItems = query.Execute();
            List<AggregateFeedItem> aggregateFeed = AggregateFeedItem.FromFeedItems(feedItems);
            this.UpdateLogSubscriptions(CurrentUser);
            vm.LastPollDate = query.StartDate;
            vm.Feed = aggregateFeed;
            vm.EventFilterOptions = ActivityFeedQuery.GetAllEvents().OrderBy(e => e.PrettyName).ToList();
            vm.UserEventFilterOptions = query.ActiveEvents;
            vm.ErrorTypes = Db.ErrorTypes.Distinct().ToList();
            vm.SelectedErrorType = new ErrorType();
            if (errorType > 0)
            {
                vm.SelectedErrorType = Db.ErrorTypes.Where(e => e.Id == errorType).FirstOrDefault();
                if (vm.SelectedErrorType == null)
                {
                    vm.SelectedErrorType = new ErrorType();
                }
            }
            return View(vm);
        }

        /// <summary>
        /// Returns a raw feed without any extra HTML chrome.  Used for AJAX updates to an existing feed.
        /// </summary>
        /// <param name="id">The ID of the last feed item received by the client</param>
        /// <returns></returns>
        public ActionResult RecentFeedItems(int id, int userId = -1)
        {            
            ActivityFeedQuery query = BuildBasicQuery();
            query.MinLogId = id;
            query.MaxQuerySize = 10;

            //used to build a feed for a single person.  Useful for building profile-based feeds
            if (userId > 0)
            {
                query.ClearSubscriptionSubjects();
                query.AddSubscriptionSubject(Db.Users.Where(u => u.Id == userId).FirstOrDefault());
            }
            List<FeedItem> feedItems = query.Execute();
            List<AggregateFeedItem> aggregateFeed = AggregateFeedItem.FromFeedItems(feedItems);
            return View("AjaxFeed", aggregateFeed);
        }

        [HttpPost]
        public ActionResult GetItemUpdates(List<GetItemUpdatesViewModel> items)
        {
            if (items != null)
            {
                List<int> logIds = items.Select(i => i.LogId).ToList();
                DateTime lastPollDate = new DateTime(items.First().LastPollTick);
                var query = from comment in Db.LogComments.Include("HelpfulMarks").Include("Author").AsNoTracking()
                            where logIds.Contains(comment.LogId)
                            && comment.DatePosted > lastPollDate
                            select comment;
                List<LogComment> comments = query.ToList();
                long lastPollTick = items[0].LastPollTick;
                var maxCommentTick = comments.Select(c => c.DatePosted);
                if(maxCommentTick.Count() > 0)
                {
                    lastPollTick = maxCommentTick.Max().Ticks;
                }
                var result = new { LastPollTick = lastPollTick, Comments = comments };
                ViewBag.LastPollTick = lastPollTick;
                return View(comments);
            }
            return View(new List<LogComment>());
        }

        /// <summary>
        /// Returns a raw feed of past feed items without any extra HTML chrome.  Used for AJAX updates to an existing feed.
        /// </summary>
        /// <param name="id">The ID of the first feed item received by the client.</param>
        /// <returns></returns>
        public ActionResult OldFeedItems(int id, int count, int userId)
        {
            ActivityFeedQuery query = BuildBasicQuery();
            query.MaxLogId = id;
            query.MaxQuerySize = count;

            //used to build a feed for a single person.  Useful for building profile-based feeds
            if (userId > 0)
            {
                query.ClearSubscriptionSubjects();
                query.AddSubscriptionSubject(Db.Users.Where(u => u.Id == userId).FirstOrDefault());
            }

            List<FeedItem> feedItems = query.Execute();
            List<AggregateFeedItem> aggregateFeed = AggregateFeedItem.FromFeedItems(feedItems);
            return View("AjaxFeed", aggregateFeed);
        }

        /// <summary>
        /// Provides a details view for the provided Log IDs
        /// </summary>
        /// <param name="id">The ID(s) of the logs to retrieve.  Accepts a comma delimited list.  
        /// In the case of rendering multiple IDs, an aggregate view will be created
        /// </param>
        /// <returns></returns>
        public ActionResult Details(string id)
        {
            //make sure that we've gotten a valid ID
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }

            ActivityFeedQuery query = new ActivityFeedQuery(Db);

            List<int> ids = ParseIdString(id);
            foreach (int logId in ids)
            {
                query.AddEventId(logId);
            }
            List<FeedItem> feedItems = query.Execute();
            List<AggregateFeedItem> aggregateItems = AggregateFeedItem.FromFeedItems(feedItems);

            FeedDetailsViewModel vm = new FeedDetailsViewModel();
            vm.Ids = id;
            vm.FeedItem = aggregateItems.FirstOrDefault();
            if (Db.EventLogSubscriptions.Where(e => e.UserId == CurrentUser.Id).Where(e => e.LogId == ids.Min()).Count() > 0)
            {
                vm.IsSubscribed = true;
            }
            return View(vm);
        }

        /// <summary>
        /// Will subscribe the active user to the event log with the supplied ID number
        /// </summary>
        /// <param name="id"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public ActionResult FollowPost(int id, string returnUrl)
        {
            int count = Db.EventLogs.Where(l => l.Id == id).Count();
            if (count > 0)
            {
                EventLogSubscription subscription = new EventLogSubscription()
                {
                    LogId = id,
                    UserId = CurrentUser.Id
                };
                try
                {
                    Db.EventLogSubscriptions.Add(subscription);
                    Db.SaveChanges();
                }
                catch (Exception)
                {
                }
            }
            Response.Redirect(returnUrl);
            return View();
        }

        public ActionResult MarkCommentHelpful(int commentId, string returnUrl)
        {
            int count = Db.HelpfulLogComments
                .Where(c => c.UserId == CurrentUser.Id)
                .Where(c => c.CommentId == commentId)
                .Count();
            if (count == 0)
            {
                LogComment comment = Db.LogComments.Where(c => c.Id == commentId).FirstOrDefault();
                if (commentId != null)
                {
                    HelpfulLogComment help = new HelpfulLogComment()
                    {
                        CommentId = commentId,
                        UserId = CurrentUser.Id
                    };
                    Db.HelpfulLogComments.Add(help);
                    Db.SaveChanges();
                }
            }
            Response.Redirect(returnUrl);
            return View();
        }

        /// <summary>
        /// Will unsubscribe the active user from the event log with the supplied ID number
        /// </summary>
        /// <param name="id"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public ActionResult UnfollowPost(int id, string returnUrl)
        {
            EventLogSubscription subscription = Db.EventLogSubscriptions.Where(s => s.UserId == CurrentUser.Id).Where(s => s.LogId == id).FirstOrDefault();
            if (subscription != null)
            {
                Db.Entry(subscription).State = System.Data.EntityState.Deleted;
                Db.SaveChanges();
            }
            Response.Redirect(returnUrl);
            return View();
        }

        /// <summary>
        /// Adds a global comment that will appear in the activity feed
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PostFeedComment(string comment)
        {
            OsbideWebService client = new OsbideWebService();
            Authentication auth = new Authentication();
            string key = auth.GetAuthenticationKey();
            if(string.IsNullOrEmpty(comment) == false)
            {
                EventLog log = new EventLog();
                log.SenderId = CurrentUser.Id;
                log.LogType = FeedCommentEvent.Name;
                FeedCommentEvent commentEvent = new FeedCommentEvent();
                commentEvent.Comment = comment;
                log.Data.BinaryData = EventFactory.ToZippedBinary(commentEvent);
                client.SubmitLog(log, key);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ApplyFeedFilter()
        {
            string errorType = "";
            if (Request.Form.AllKeys.Contains("error-type"))
            {
                errorType = Request.Form["error-type"];
            }

            UserFeedSetting feedSetting = Db.UserFeedSettings.Where(u => u.UserId == CurrentUser.Id).FirstOrDefault();
            if (feedSetting == null)
            {
                feedSetting = new UserFeedSetting();
                feedSetting.UserId = CurrentUser.Id;
                Db.UserFeedSettings.Add(feedSetting);
            }

            //clear out existing settings
            feedSetting.Settings = 0;

            //load in new settings
            foreach (string key in Request.Form.Keys)
            {
                if (key.StartsWith("event_") == true)
                {
                    string[] pieces = key.Split('_');
                    if (pieces.Length == 2)
                    {
                        IOsbideEvent evt = EventFactory.FromName(pieces[1]);
                        if (evt != null)
                        {
                            feedSetting.SetSetting(evt, true);
                        }
                    }
                }
            }

            //save changes
            Db.SaveChanges();

            return RedirectToAction("Index", new { errorType = errorType });
        }

        /// <summary>
        /// Constructs a basic query to be further manipulated by other functions in this class
        /// </summary>
        /// <returns></returns>
        private ActivityFeedQuery BuildBasicQuery()
        {
            //pull down the current user's list of subscriptions
            List<OsbideUser> subscriptions = new StudentSubscriptionsQuery(Db, CurrentUser).Execute();

            //and add himself to the list as well (so that his posts show up in the feed)
            subscriptions.Add(CurrentUser);

            //now, make the query
            ActivityFeedQuery query = new ActivityFeedQuery(Db);

            //add the event types that the user wants to see
            UserFeedSetting feedSettings = Db.UserFeedSettings.Where(u => u.UserId == CurrentUser.Id).FirstOrDefault();
            if (feedSettings == null)
            {
                foreach (IOsbideEvent evt in ActivityFeedQuery.GetAllEvents())
                {
                    query.AddEventType(evt);
                }
            }
            else
            {
                foreach (FeedSetting setting in feedSettings.ActiveSettings)
                {
                    query.AddEventType(UserFeedSetting.FeedOptionToOsbideEvent(setting));
                }
            }

            //add in the list of users that the current person cares about
            query.AddSubscriptionSubject(subscriptions);
            return query;
        }

    }

}
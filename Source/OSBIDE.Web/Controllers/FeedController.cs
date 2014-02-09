﻿using DiffMatchPatch;
using OSBIDE.Library;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using OSBIDE.Web.Models;
using OSBIDE.Web.Models.Attributes;
using OSBIDE.Web.Models.Queries;
using OSBIDE.Web.Models.ViewModels;
using OSBIDE.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    [OsbideAuthorize]
    [RequiresVisualStudioConnectionForStudents]
    public class FeedController : ControllerBase
    {
        
        private UserFeedSetting _userSettings = new UserFeedSetting();
        public FeedController()
        {
            _userSettings = (from setting in Db.UserFeedSettings
                             where setting.UserId == CurrentUser.Id
                             orderby setting.Id descending
                             select setting)
                            .Take(1)
                            .FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">The ID of the last event received by the user.  Used for AJAX updates</param>
        /// <returns></returns>
        public ActionResult Index(long timestamp = -1, int errorType = -1, string errorTypeStr = "")
        {
            //turned off for now.
            //return RedirectToAction("FeedDown", "Error");

            ActivityFeedQuery query = new ActivityFeedQuery(Db);

            //Two ways that we can receive an error type: by name (errorTypeStr) or by ID (errorType).
            //First, we check the string and see if we can match it to an ID number.  Then, we check
            //to see if we have a valid ID number.  If it doesn't work out, just work as normal.
            if (string.IsNullOrEmpty(errorTypeStr) == false)
            {
                errorTypeStr = errorTypeStr.ToLower().Trim();
                ErrorType type = Db.ErrorTypes.Where(e => e.Name.CompareTo(errorTypeStr) == 0).FirstOrDefault();
                if (type != null)
                {
                    errorType = type.Id;
                }
            }
            if (errorType > 0)
            {
                query = new BuildErrorQuery(Db);
                (query as BuildErrorQuery).BuildErrorTypeId = errorType;
            }
            BuildBasicQuery(query);
            FeedViewModel vm = new FeedViewModel();

            if (timestamp > 0)
            {
                DateTime pullDate = new DateTime(timestamp);
                query.StartDate = pullDate;
            }
            else
            {
                query.StartDate = DateTime.MinValue;
                query.MaxQuerySize = 40;
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
            List<FeedItem> feedItems = query.Execute().ToList();
            List<AggregateFeedItem> aggregateFeed = AggregateFeedItem.FromFeedItems(feedItems);
            this.UpdateLogSubscriptions(CurrentUser);
            try
            {
                vm.LastPollDate = aggregateFeed.Select(a => a.MostRecentOccurance).Max();
            }
            catch (Exception)
            {
                vm.LastPollDate = DateTime.MinValue.AddDays(2);
            }
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

            //build possible courses and user types
            vm.Courses = Db.Courses.ToList();
            vm.CourseRoles.Add(CourseRole.Student);
            vm.CourseRoles.Add(CourseRole.Assistant);
            vm.CourseRoles.Add(CourseRole.Coordinator);
            if (_userSettings != null)
            {
                vm.SelectedCourseId = _userSettings.CourseFilter;
                vm.SelectedCourseRole = _userSettings.CourseRole;
            }

            //build the "you and 5 others got this error"-type messages
            BuildEventRelations(vm, feedItems);

            return View(vm);
        }

        private void BuildEventRelations(FeedViewModel vm, List<FeedItem> feedItems)
        {
            //build the "you and 5 others got this error"-type messages
            vm.RecentUserErrors = base.GetRecentCompileErrors(CurrentUser).ToList();

            //This code does the following:
            // 1. Find all errors being sent out
            // 2. Find students who have recently had these errors
            // 3. Add this info to our VM

            //step 1
            List<BuildEvent> feedBuildEvents = feedItems
                .Where(i => i.Log.LogType.CompareTo(BuildEvent.Name) == 0)
                .Select(i => i.Event)
                .Cast<BuildEvent>()
                .ToList();
            SortedDictionary<string, string> sortedFeedBuildErrors = new SortedDictionary<string, string>();
            List<string> feedBuildErrors;
            foreach (BuildEvent build in feedBuildEvents)
            {
                foreach (BuildEventErrorListItem errorItem in build.ErrorItems)
                {
                    string key = errorItem.ErrorListItem.CriticalErrorName;
                    if (string.IsNullOrEmpty(key) == false)
                    {
                        if (sortedFeedBuildErrors.ContainsKey(key) == false)
                        {
                            sortedFeedBuildErrors.Add(key, key);
                        }
                    }
                }
            }

            //convert the above to a normal list once we're done
            feedBuildErrors = sortedFeedBuildErrors.Keys.ToList();

            //step 2: find other students who have also had these errors
            List<UserBuildErrorsByType> classBuildErrors = new List<UserBuildErrorsByType>();
            DateTime maxLookback = base.DefaultErrorLookback;
            var classBuilds = (from buildError in Db.BuildErrors
                               where feedBuildErrors.Contains(buildError.BuildErrorType.Name)
                               && buildError.Log.DateReceived > maxLookback
                               && buildError.Log.SenderId != CurrentUser.Id
                               group buildError by buildError.BuildErrorType.Name into be
                               select new { ErrorName = be.Key, Users = be.Select(s => s.Log.Sender).Distinct() }).ToList();

            foreach (var item in classBuilds)
            {
                classBuildErrors.Add(new UserBuildErrorsByType()
                {
                    Users = item.Users.ToList(),
                    ErrorName = item.ErrorName
                });
            }

            vm.RecentClassErrors = classBuildErrors;
        }

        /// <summary>
        /// Returns a raw feed without any extra HTML chrome.  Used for AJAX updates to an existing feed.
        /// </summary>
        /// <param name="id">The ID of the last feed item received by the client</param>
        /// <returns></returns>
        public ActionResult RecentFeedItems(int id, int userId = -1, int errorType = -1)
        {
            //return View("AjaxFeed", new List<AggregateFeedItem>()); 

            ActivityFeedQuery query = new ActivityFeedQuery(Db);
            if (errorType > 0)
            {
                query = new BuildErrorQuery(Db);
                (query as BuildErrorQuery).BuildErrorTypeId = errorType;
            }
            BuildBasicQuery(query);
            query.MinLogId = id;
            query.MaxQuerySize = 10;

            //used to build a feed for a single person.  Useful for building profile-based feeds
            if (userId > 0)
            {
                query.ClearSubscriptionSubjects();
                query.AddSubscriptionSubject(Db.Users.Where(u => u.Id == userId).FirstOrDefault());
            }
            List<FeedItem> feedItems = query.Execute().ToList();
            List<AggregateFeedItem> aggregateFeed = AggregateFeedItem.FromFeedItems(feedItems);

            //build the "you and 5 others got this error"-type messages
            FeedViewModel vm = new FeedViewModel();
            BuildEventRelations(vm, feedItems);

            ViewBag.RecentUserErrors = vm.RecentUserErrors;
            ViewBag.RecentClassErrors = vm.RecentClassErrors;
            ViewBag.ErrorTypes = vm.ErrorTypes;

            return View("AjaxFeed", aggregateFeed);
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult PostCommentAsync(string logId, string comment)
        {
            int id = -1;
            if (Int32.TryParse(logId, out id))
            {
                bool result = base.PostComment(logId, comment);
                return GetComments(id);
            }
            return this.Json(new { });
        }

        public JsonResult GetComments(int? singleLogId)
        {
            //turned off for now
            //return this.Json(new { Data = new{} }, JsonRequestBehavior.AllowGet);

            List<int> logIds = new List<int>();
            try
            {
                logIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(Request.Form["logIds"]);
            }
            catch (Exception)
            {
                logIds = new List<int>();
            }

            //legacy code will send a single log Id.  In that case, add it to the list of log ids
            if (singleLogId != null)
            {
                logIds.Add((int)singleLogId);
            }

            //for each log Id, build the appropriate comments view model
            Dictionary<int, List<CommentsViewModel>> viewModels = new Dictionary<int, List<CommentsViewModel>>();
            List<object> jsonVm = new List<object>();
            foreach (int logId in logIds)
            {
                EventLog log = Db.EventLogs.Where(l => l.Id == logId).FirstOrDefault();
                int actualLogId = logId;
                if (log != null)
                {
                    IList<LogCommentEvent> comments = new List<LogCommentEvent>();

                    //log comment events and mark helpful events need to pull their comments from the log to which they're attached.  All others
                    //pull from themselves.
                    if (log.LogType == LogCommentEvent.Name)
                    {
                        LogCommentEvent evt = Db.LogCommentEvents.Where(c => c.EventLogId == log.Id).FirstOrDefault();
                        if (evt != null)
                        {
                            actualLogId = evt.SourceEventLogId;
                            comments = evt.SourceEventLog.Comments;
                        }
                    }
                    else if (log.LogType == HelpfulMarkGivenEvent.Name)
                    {
                        HelpfulMarkGivenEvent evt = Db.HelpfulMarkGivenEvents.Where(h => h.EventLogId == log.Id).FirstOrDefault();
                        if (evt != null)
                        {
                            actualLogId = evt.LogCommentEvent.SourceEventLogId;
                            comments = evt.LogCommentEvent.SourceEventLog.Comments;
                        }
                    }
                    else
                    {
                        comments = log.Comments;
                    }

                    //ignore duplicate log Ids
                    if(viewModels.ContainsKey(actualLogId) == true)
                    {
                        continue;
                    }
                    viewModels.Add(actualLogId, new List<CommentsViewModel>());

                    //sort comments
                    comments = comments.OrderBy(c => c.EventDate).ToList();

                    //convert LogCommentEvents into JSON
                    foreach (LogCommentEvent comment in comments)
                    {
                        CommentsViewModel c = new CommentsViewModel()
                        {
                            CommentId = comment.Id,
                            CourseName = comment.EventLog.Sender.DefaultCourse.Name,
                            Content = comment.Content,
                            FirstAndLastName = comment.EventLog.Sender.FirstAndLastName,
                            ProfileUrl = Url.Action("Picture", "Profile", new { id = comment.EventLog.SenderId, size = 48 }),
                            UtcEventDate = comment.EventDate,
                            MarkHelpfulCount = comment.HelpfulMarks.Count(),
                            MarkHelpfulUrl = Url.Action("MarkCommentHelpful", "Feed", new { commentId = comment.Id, returnUrl = Url.Action("Index", "Feed") })
                        };

                        //check to see if the current user is allowed to mark the comment as helpful
                        HelpfulMarkGivenEvent helpful = comment.HelpfulMarks.Where(m => m.EventLog.SenderId == CurrentUser.Id).FirstOrDefault();
                        if (helpful == null && comment.EventLog.SenderId != CurrentUser.Id)
                        {
                            c.DisplayHelpfulMarkLink = true;
                        }

                        //add to VM
                        viewModels[actualLogId].Add(c);
                    }

                    //convert to json view model
                    jsonVm.Add(new { Comments = viewModels[actualLogId], ActualLogId = actualLogId, OriginalLogId = logId });
                }
            }
            return this.Json(new { Data = jsonVm }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetItemUpdates(List<GetItemUpdatesViewModel> items)
        {
            if (items != null)
            {
                List<int> logIds = items.Select(i => i.LogId).ToList();
                DateTime lastPollDate = new DateTime(items.First().LastPollTick);
                var query = from comment in Db.LogCommentEvents.Include("HelpfulMarks").Include("EventLog").Include("EventLog.Sender").AsNoTracking()
                            where
                            (logIds.Contains(comment.SourceEventLogId) || logIds.Contains(comment.EventLogId))
                            && comment.EventDate > lastPollDate
                            select comment;
                List<LogCommentEvent> comments = query.ToList();
                long lastPollTick = items[0].LastPollTick;
                var maxCommentTick = comments.Select(c => c.EventDate);
                if (maxCommentTick.Count() > 0)
                {
                    lastPollTick = maxCommentTick.Max().Ticks;
                }
                var result = new { LastPollTick = lastPollTick, Comments = comments };
                ViewBag.LastPollTick = lastPollTick;
                return View(comments);
            }
            return View(new List<LogCommentEvent>());
        }

        /// <summary>
        /// Returns a raw feed of past feed items without any extra HTML chrome.  Used for AJAX updates to an existing feed.
        /// </summary>
        /// <param name="id">The ID of the first feed item received by the client.</param>
        /// <returns></returns>
        public ActionResult OldFeedItems(int id, int count, int userId, int errorType = -1)
        {
            ActivityFeedQuery query = new ActivityFeedQuery(Db);
            if (errorType > 0)
            {
                query = new BuildErrorQuery(Db);
                (query as BuildErrorQuery).BuildErrorTypeId = errorType;
            }
            BuildBasicQuery(query);
            query.MaxLogId = id;
            query.MaxQuerySize = count;

            //used to build a feed for a single person.  Useful for building profile-based feeds
            if (userId > 0)
            {
                query.ClearSubscriptionSubjects();
                query.AddSubscriptionSubject(Db.Users.Where(u => u.Id == userId).FirstOrDefault());
            }

            List<FeedItem> feedItems = query.Execute().ToList();
            List<AggregateFeedItem> aggregateFeed = AggregateFeedItem.FromFeedItems(feedItems);


            //build the "you and 5 others got this error"-type messages
            FeedViewModel vm = new FeedViewModel();
            BuildEventRelations(vm, feedItems);

            ViewBag.RecentUserErrors = vm.RecentUserErrors;
            ViewBag.RecentClassErrors = vm.RecentClassErrors;
            ViewBag.ErrorTypes = vm.ErrorTypes;

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

            //check to receive if we've gotten a single ID back
            int idAsInt = -1;
            if (Int32.TryParse(id, out idAsInt) == true)
            {
                //if we've received a log comment event or a helpful mark event, we have to reroute to the original event
                EventLog log = Db.EventLogs.Where(e => e.Id == idAsInt).FirstOrDefault();
                if (log != null)
                {
                    if (log.LogType == LogCommentEvent.Name)
                    {
                        LogCommentEvent commentEvent = Db.LogCommentEvents.Where(c => c.EventLogId == log.Id).FirstOrDefault();
                        return RedirectToAction("Details", "Feed", new { id = commentEvent.SourceEventLogId });
                    }
                    else if (log.LogType == HelpfulMarkGivenEvent.Name)
                    {
                        HelpfulMarkGivenEvent helpfulEvent = Db.HelpfulMarkGivenEvents.Where(e => e.EventLogId == log.Id).FirstOrDefault();
                        return RedirectToAction("Details", "Feed", new { id = helpfulEvent.LogCommentEvent.SourceEventLogId });
                    }
                }
            }

            ActivityFeedQuery query = new ActivityFeedQuery(Db);

            List<int> ids = ParseIdString(id);
            foreach (int logId in ids)
            {
                query.AddEventId(logId);
            }
            List<FeedItem> feedItems = query.Execute().ToList();
            List<AggregateFeedItem> aggregateItems = AggregateFeedItem.FromFeedItems(feedItems);

            //build the "you and 5 others got this error"-type messages
            FeedViewModel fvm = new FeedViewModel();
            BuildEventRelations(fvm, feedItems);

            ViewBag.RecentUserErrors = fvm.RecentUserErrors;
            ViewBag.RecentClassErrors = fvm.RecentClassErrors;
            ViewBag.ErrorTypes = fvm.ErrorTypes;

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
            int count = Db.HelpfulMarkGivenEvents
                .Where(c => c.EventLog.SenderId == CurrentUser.Id)
                .Where(c => c.LogCommentEventId == commentId)
                .Count();
            if (count == 0)
            {
                LogCommentEvent comment = Db.LogCommentEvents.Where(c => c.Id == commentId).FirstOrDefault();
                if (commentId != 0)
                {
                    HelpfulMarkGivenEvent help = new HelpfulMarkGivenEvent()
                    {
                        LogCommentEventId = commentId
                    };
                    OsbideWebService client = new OsbideWebService();
                    Authentication auth = new Authentication();
                    string key = auth.GetAuthenticationKey();
                    EventLog log = new EventLog(help, CurrentUser);
                    client.SubmitLog(log, CurrentUser);
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
                Db.Entry(subscription).State = System.Data.Entity.EntityState.Deleted;
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
        [ValidateInput(false)]
        public ActionResult PostFeedComment(string comment)
        {
            OsbideWebService client = new OsbideWebService();
            Authentication auth = new Authentication();
            string key = auth.GetAuthenticationKey();
            if (string.IsNullOrEmpty(comment) == false)
            {
                EventLog log = new EventLog();
                log.SenderId = CurrentUser.Id;
                log.LogType = FeedPostEvent.Name;
                FeedPostEvent commentEvent = new FeedPostEvent();
                commentEvent.Comment = comment;
                log.Data.BinaryData = EventFactory.ToZippedBinary(commentEvent);
                log = client.SubmitLog(log, CurrentUser);

                //find all of this user's subscribers and send them an email
                List<OsbideUser> observers = new List<OsbideUser>();

                observers = (from subscription in Db.UserSubscriptions
                             join user in Db.Users on
                                               new { InstitutionId = subscription.ObserverInstitutionId, SchoolId = subscription.ObserverSchoolId }
                                               equals new { InstitutionId = user.InstitutionId, SchoolId = user.SchoolId }
                             where subscription.SubjectSchoolId == CurrentUser.SchoolId
                                && subscription.SubjectInstitutionId == CurrentUser.InstitutionId
                                && user.ReceiveEmailOnNewFeedPost == true
                             select user).ToList();
                if (observers.Count > 0)
                {
                    string url = StringConstants.GetActivityFeedDetailsUrl(log.Id);
                    string body = "Greetings,<br />{0} posted a new item to the activity feed:<br />\"{1}\"<br />To view this "
                    + "conversation online, please visit {2} or visit your OSBIDE user profile.<br /><br />Thanks,\nOSBIDE<br /><br />"
                    + "These automated messages can be turned off by editing your user profile.";
                    body = string.Format(body, CurrentUser.FirstAndLastName, comment, url);
                    List<MailAddress> to = new List<MailAddress>();
                    foreach (OsbideUser user in observers)
                    {
                        to.Add(new MailAddress(user.Email));
                    }
                    Email.Send("[OSBIDE] New Activity Post", body, to);
                }
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

            UserFeedSetting feedSetting = _userSettings;
            if (feedSetting == null)
            {
                feedSetting = new UserFeedSetting();
                feedSetting.UserId = CurrentUser.Id;
            }
            else
            {
                feedSetting = new UserFeedSetting(feedSetting);
                feedSetting.Id = 0;
                feedSetting.SettingsDate = DateTime.UtcNow;
            }
            Db.UserFeedSettings.Add(feedSetting);

            //clear out existing settings
            feedSetting.EventFilterSettings = 0;

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

            //check for course filter
            if (Request.Form.AllKeys.Contains("course-filter"))
            {
                int courseId = -1;
                Int32.TryParse(Request.Form["course-filter"], out courseId);
                feedSetting.CourseFilter = courseId;
            }

            //check for user filter
            if (Request.Form.AllKeys.Contains("user-type-filter"))
            {
                int userRoleId = (int)CourseRole.Student;
                Int32.TryParse(Request.Form["user-type-filter"], out userRoleId);
                feedSetting.CourseRoleFilter = userRoleId;
            }

            //save changes
            Db.SaveChanges();

            return RedirectToAction("Index", new { errorType = errorType });
        }

        /// <summary>
        /// Constructs a basic query to be further manipulated by other functions in this class
        /// </summary>
        /// <returns></returns>
        private ActivityFeedQuery BuildBasicQuery(ActivityFeedQuery query = null)
        {
            //check for null query
            if (query == null)
            {
                query = new ActivityFeedQuery(Db);
            }

            //add the event types that the user wants to see
            UserFeedSetting feedSettings = _userSettings;
            if (feedSettings == null || feedSettings.ActiveSettings.Count == 0)
            {
                foreach (IOsbideEvent evt in ActivityFeedQuery.GetAllEvents())
                {
                    query.AddEventType(evt);
                }
            }
            else
            {
                //load in event filter settings
                foreach (EventFilterSetting setting in feedSettings.ActiveSettings)
                {
                    query.AddEventType(UserFeedSetting.FeedOptionToOsbideEvent(setting));
                }

                //load in course and user type filtering
                query.CourseRoleFilter = (CourseRole)feedSettings.CourseRoleFilter;
                query.CourseFilter = new Course() { Id = feedSettings.CourseFilter };
            }

            return query;
        }

    }

}
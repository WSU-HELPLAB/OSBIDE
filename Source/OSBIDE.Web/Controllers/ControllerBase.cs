using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OSBIDE.Library.Models;
using OSBIDE.Web.Models;
using System.Text.RegularExpressions;
using System.Runtime.Caching;
using OSBIDE.Library.Events;

namespace OSBIDE.Web.Controllers
{
    public abstract class ControllerBase : Controller
    {
        protected OsbideContext Db { get; private set; }
        protected OsbideUser CurrentUser { get; set; }
        protected FileCache GlobalCache { get; private set; }
        protected FileCache UserCache { get; private set; }

        public static string DefaultConnectionString
        {
            get
            {
                string conn = "";
#if DEBUG
                conn = System.Configuration.ConfigurationManager.ConnectionStrings["OsbideDebugContext"].ConnectionString;
#else
                conn = System.Configuration.ConfigurationManager.ConnectionStrings["OsbideReleaseContext"].ConnectionString;
#endif
                return conn;
            }
        }

        public ControllerBase()
        {
            //set up DB
            Db = OsbideContext.DefaultWebConnection;

            //set up current user
            Authentication auth = new Authentication();
            string authKey = auth.GetAuthenticationKey();
            CurrentUser = auth.GetActiveUser(authKey);
            Db.Users.Attach(CurrentUser);
            CurrentUser.PropertyChanged += CurrentUser_PropertyChanged;

            //set up caches
            GlobalCache = FileCacheHelper.GetGlobalCacheInstance();
            UserCache = FileCacheHelper.GetCacheInstance(CurrentUser);

            //update all users scores if necessary
            object lastScoreUpdate = GlobalCache["lastScoreUpdate"];
            bool needsScoreUpdate = true;
            if (lastScoreUpdate != null)
            {
                DateTime lastUpdate = (DateTime)lastScoreUpdate;
                if (lastUpdate.AddDays(1) > DateTime.UtcNow)
                {
                    needsScoreUpdate = false;
                }
            }
            if (needsScoreUpdate == true)
            {
                UpdateUserScores();
                GlobalCache["lastScoreUpdate"] = DateTime.UtcNow;
            }

            //make current user available to all views
            ViewBag.CurrentUser = CurrentUser;
        }

        /// <summary>
        /// Updates all users' scores and saves the information in the database
        /// </summary>
        private void UpdateUserScores()
        {
            //scoring:
            // 1 pt for each FeedPostEvent / AskForHelpEvent
            // 3 pts for each log comment
            // 7 pts for each helpful mark
            var onePtQuery = from log in Db.EventLogs
                             where (log.LogType == FeedPostEvent.Name || log.LogType == AskForHelpEvent.Name)
                             group log by log.SenderId into logGroup
                             select new { UserId = logGroup.Key, LogCount = logGroup.Count() };

            var threePtQuery = from comment in Db.LogCommentEvents
                               group comment by comment.EventLog.SenderId into commentGroup
                               select new { UserId = commentGroup.Key, CommentCount = commentGroup.Count() };

            var sevenPtQuery = from helpful in Db.HelpfulMarkGivenEvents
                               group helpful by helpful.LogCommentEvent.EventLog.SenderId into helpfulGroup
                               select new { UserId = helpfulGroup.Key, HelpfulCount = helpfulGroup.Count() };

            Dictionary<int, int> scores = new Dictionary<int, int>();
            foreach (var row in onePtQuery)
            {
                if (scores.ContainsKey(row.UserId) == false)
                {
                    scores.Add(row.UserId, 0);
                }
                scores[row.UserId] += row.LogCount;
            }

            foreach (var row in threePtQuery)
            {
                if (scores.ContainsKey(row.UserId) == false)
                {
                    scores.Add(row.UserId, 0);
                }
                scores[row.UserId] += (row.CommentCount * 3);
            }
            foreach (var row in sevenPtQuery)
            {
                if (scores.ContainsKey(row.UserId) == false)
                {
                    scores.Add(row.UserId, 0);
                }
                scores[row.UserId] += (row.HelpfulCount * 7);
            }

            //remove all user scores
            Db.DeleteUserScores();

            //add in new ones
            foreach (int userKey in scores.Keys)
            {
                UserScore score = new UserScore()
                {
                    UserId = userKey,
                    Score = scores[userKey],
                    LastCalculated = DateTime.UtcNow
                };
                Db.UserScores.Add(score);
            }
            Db.SaveChanges();
        }

        /// <summary>
        /// Updates the event log subscription list for the supplied user.  
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public OsbideUser UpdateLogSubscriptions(OsbideUser user)
        {
            user.LogSubscriptions = Db.EventLogSubscriptions.Where(u => u.UserId == user.Id).ToList();
            return user;
        }

        /// <summary>
        /// Called whenever the system modifies the current user.  This function will update the DB and also the local cookie-based cache
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentUser_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Db.Entry(CurrentUser).State = System.Data.EntityState.Modified;
            try
            {
                Db.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
            }
            Authentication auth = new Authentication();
            auth.LogIn(CurrentUser);
        }

        /// <summary>
        /// Will return a list of recent compile errors for the given user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="timeframe">How far back the system should look</param>
        /// <returns></returns>
        protected string[] GetRecentCompileErrors(OsbideUser user, DateTime timeframe)
        {
            List<string> errors = new List<string>();

            List<ErrorListItem> errorItems = (from log in Db.EventLogs
                                              join build in Db.BuildEvents on log.Id equals build.EventLogId
                                              join buildError in Db.BuildEventErrorListItems on build.Id equals buildError.BuildEventId
                                              join error in Db.ErrorListItems on buildError.ErrorListItemId equals error.Id
                                              where
                                                error.Description.ToLower().StartsWith("error") == true
                                                && log.SenderId == user.Id
                                                && log.DateReceived > timeframe
                                              select error).ToList();
            return errorItems.Where(e => e.CriticalErrorName.Length > 0).Select(e => e.CriticalErrorName).ToArray();
        }
        
        /// <summary>
        /// Will return a list of recent compile errors for the given user.  Will pull errors that occurred within the last
        /// 48 hours.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        protected string[] GetRecentCompileErrors(OsbideUser user)
        {
            return GetRecentCompileErrors(user, DefaultErrorLookback);
        }

        /// <summary>
        /// The default amount of time to "look back" for similar errors caused by other users
        /// </summary>
        protected DateTime DefaultErrorLookback
        {
            get
            {
                return DateTime.UtcNow.Subtract(new TimeSpan(0, 48, 0, 0, 0));
            }
        }

        protected List<int> ParseIdString(string idStr)
        {
            //get out list of ID numbers
            string[] rawIds = idStr.Split(',');
            List<int> ids = new List<int>(rawIds.Length);
            for (int i = 0; i < rawIds.Length; i++)
            {
                int tempId = -1;
                if (Int32.TryParse(rawIds[i], out tempId) == true)
                {
                    ids.Add(tempId);
                }
            }
            return ids;
        }

    }
}

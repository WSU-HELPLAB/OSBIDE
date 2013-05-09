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
                if (lastUpdate.AddDays(1) > DateTime.Now)
                {
                    needsScoreUpdate = false;
                }
            }
            if (needsScoreUpdate == true)
            {
                UpdateUserScores();
                GlobalCache["lastScoreUpdate"] = DateTime.Now;
            }

            //make current user available to all views
            ViewBag.CurrentUser = CurrentUser;
        }

        /// <summary>
        /// Updates all users' scores and saves the information in the database
        /// </summary>
        private void UpdateUserScores()
        {
            //TODO: This query doesn't work right as it gives helpful marks to the person who makred as helpful instead of
            //the original author
            var query = from user in Db.Users
                        join comment2 in Db.LogComments on user.Id equals comment2.AuthorId into comments
                        join helpfulMark in Db.HelpfulLogComments on user.Id equals helpfulMark.UserId into helpfulComments
                        join log in Db.EventLogs on user.Id equals log.SenderId into logs
                        join score in Db.UserScores on user.Id equals score.UserId into scores
                        select new
                        {
                            User = user,
                            LogCount = logs.Count(),
                            CommentsCount = comments.Count(),
                            HelpfulCommentsCount = helpfulComments.Count(),
                            UserScore = scores.FirstOrDefault()
                        };
            foreach (var entry in query)
            {
                UserScore score = entry.UserScore;
                if (score == null)
                {
                    score = new UserScore()
                    {
                        UserId = entry.User.Id,
                    };
                    Db.UserScores.Add(score);
                }
                score.Score = (1 * entry.LogCount) + (3 * entry.CommentsCount) + (7 * entry.HelpfulCommentsCount);
                score.LastCalculated = DateTime.Now;
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
                                              select error).ToList();
            
            //now that we have the errors, pull out the code
            string pattern = "error ([^:]+)";
            foreach (ErrorListItem item in errorItems)
            {
                Match match = Regex.Match(item.Description, pattern);
                
                //ignore bad matches
                if (match.Groups.Count == 2)
                {
                    string errorCode = match.Groups[1].Value;
                    if (errorCode.Length > 0 && errors.Contains(errorCode) == false)
                    {
                        errors.Add(errorCode);
                    }
                }
            }
            return errors.ToArray();
        }

        /// <summary>
        /// Will return a list of recent compile errors for the given user.  Will pull errors that occurred within the last
        /// 24 hours.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        protected string[] GetRecentCompileErrors(OsbideUser user)
        {
            return GetRecentCompileErrors(user, DateTime.Now.Subtract(new TimeSpan(0, 24, 0, 0, 0)));
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

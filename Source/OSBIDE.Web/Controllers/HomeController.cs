using DiffMatchPatch;
using OSBIDE.Library;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using OSBIDE.Web.Models;
using OSBIDE.Web.Models.ViewModels;
using OSBIDE.Web.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    public class HomeController : ControllerBase
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public int PostComment(string logId, string comment, string returnUrl)
        {
            int id = -1;
            if (Int32.TryParse(logId, out id) == true)
            {
                LogCommentEvent logComment = new LogCommentEvent()
                {
                    Content = comment,
                    SourceEventLogId = id,
                    SolutionName = "OSBIDE"
                };
                OsbideWebService client = new OsbideWebService();
                Authentication auth = new Authentication();
                string key = auth.GetAuthenticationKey();
                EventLog log = null;
                if (string.IsNullOrEmpty(comment) == false)
                {
                    log = new EventLog(logComment, CurrentUser);
                    log = client.SubmitLog(log, CurrentUser);
                }
                else
                {
                    return -1;
                }
                logComment = Db.LogCommentEvents.Where(l => l.EventLogId == log.Id).FirstOrDefault();

                //the code below performs two functions:
                // 1. Send interested parties email notifications
                // 2. Log the comment in the social activity log (displayed on an individual's profile page)

                //find others that have posted on this same thread
                List<OsbideUser> interestedParties = Db.LogCommentEvents
                    .Where(l => l.SourceEventLogId == id)
                    .Where(l => l.EventLog.SenderId != CurrentUser.Id)
                    .Select(l => l.EventLog.Sender)
                    .ToList();

                //(email only) find those that are subscribed to this thread
                List<OsbideUser> subscribers = (from logSub in Db.EventLogSubscriptions
                                                join user in Db.Users on logSub.UserId equals user.Id
                                                where logSub.LogId == id
                                                && logSub.UserId != CurrentUser.Id
                                                && user.ReceiveNotificationEmails == true
                                                select user).ToList();

                //check to see if the author wants to be notified of posts
                OsbideUser eventAuthor = Db.EventLogs.Where(l => l.Id == id).Select(l => l.Sender).FirstOrDefault();

                //master list shared between email and social activity log
                SortedDictionary<int, OsbideUser> masterList = new SortedDictionary<int, OsbideUser>();
                if (eventAuthor != null)
                {
                    masterList.Add(eventAuthor.Id, eventAuthor);
                }
                foreach (OsbideUser user in interestedParties)
                {
                    if (masterList.ContainsKey(user.Id) == false)
                    {
                        masterList.Add(user.Id, user);
                    }
                }

                //add the current user for activity log tracking, but not for emails
                OsbideUser creator = new OsbideUser(CurrentUser);
                creator.ReceiveNotificationEmails = false;  //force no email send on the current user
                masterList.Add(creator.Id, creator);

                //update social activity
                foreach(OsbideUser user in masterList.Values)
                {
                    CommentActivityLog social = new CommentActivityLog()
                    {
                        TargetUserId = user.Id,
                        LogCommentEventId = logComment.Id
                    };
                    Db.CommentActivityLogs.Add(social);
                }
                Db.SaveChanges();

                //form the email list
                SortedDictionary<int, OsbideUser> emailList = new SortedDictionary<int, OsbideUser>();

                //add in interested parties from our master list
                foreach (OsbideUser user in masterList.Values)
                {
                    if (user.ReceiveNotificationEmails == true)
                    {
                        if (emailList.ContainsKey(user.Id) == false)
                        {
                            emailList.Add(user.Id, user);
                        }
                    }
                }

                //add in subscribers to email list
                foreach (OsbideUser user in subscribers)
                {
                    if (emailList.ContainsKey(user.Id) == false)
                    {
                        emailList.Add(user.Id, user);
                    }
                }

                //send emails
                if (emailList.Count > 0)
                {
                    //send email
                    string url = StringConstants.GetActivityFeedDetailsUrl(id);
                    string body = "Greetings,\n{0} has commented on a post that you have previously been involved with:\n\"{1}\"\nTo view this "
                    + "conversation online, please visit {2} or visit your OSBIDE user profile.\n\nThanks,\nOSBIDE\n\n"
                    + "These automated messages can be turned off by editing your user profile.";
                    body = string.Format(body, logComment.EventLog.Sender.FirstAndLastName, logComment.Content, url);
                    List<MailAddress> to = new List<MailAddress>();
                    foreach (OsbideUser user in emailList.Values)
                    {
                        to.Add(new MailAddress(user.Email));
                    }
                    Email.Send("[OSBIDE] Activity Notification", body, to);
                }
            }

            Response.Redirect(returnUrl);
            return id;
        }
    }
}

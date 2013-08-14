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
                if (string.IsNullOrEmpty(comment) == false)
                {
                    EventLog log = new EventLog(logComment, CurrentUser);
                    client.SubmitLog(log, key);
                }

                //notify interested parties via email

                //find others that have posted on this same thread
                List<OsbideUser> otherComments = Db.LogCommentEvents
                    .Where(l => l.SourceEventLogId == id)
                    .Where(l => l.EventLog.SenderId != CurrentUser.Id)
                    .Where(l => l.EventLog.Sender.ReceiveNotificationEmails == true)
                    .Select(l => l.EventLog.Sender)
                    .ToList();

                //find those that are subscribed to this thread
                List<OsbideUser> subscribers = (from logSub in Db.EventLogSubscriptions
                                                join user in Db.Users on logSub.UserId equals user.Id
                                                where logSub.LogId == id
                                                && logSub.UserId != CurrentUser.Id
                                                && user.ReceiveNotificationEmails == true
                                                select user).ToList();

                //check to see if the author wants to be notified of posts
                OsbideUser eventAuthor = Db.EventLogs.Where(l => l.Id == id).Select(l => l.Sender).FirstOrDefault();
                                               
                //merge the three lists together
                SortedList<int, OsbideUser> emailList = new SortedList<int, OsbideUser>();
                if (eventAuthor != null)
                {
                    if (eventAuthor.ReceiveNotificationEmails == true && eventAuthor.Id != CurrentUser.Id)
                    {
                        emailList.Add(eventAuthor.Id, eventAuthor);
                    }
                }
                foreach (OsbideUser user in otherComments)
                {
                    if (emailList.ContainsKey(user.Id) == false)
                    {
                        emailList.Add(user.Id, user);
                    }
                }
                foreach (OsbideUser user in subscribers)
                {
                    if (emailList.ContainsKey(user.Id) == false)
                    {
                        emailList.Add(user.Id, user);
                    }
                }

                
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

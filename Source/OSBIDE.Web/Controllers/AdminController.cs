using System.Globalization;
using OSBIDE.Library.CSV;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using OSBIDE.Web.Models.Attributes;
using OSBIDE.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OSBIDE.Data.DomainObjects;
using OSBIDE.Web.Models.FileSystem;
namespace OSBIDE.Web.Controllers
{
    [AllowAccess(SystemRole.Instructor)]
    public class AdminController : ControllerBase
    {
        //
        // GET: /Admin/

        public ActionResult Index()
        {
            return View();
        }

        [DenyAccess]
        [AllowAccess(SystemRole.Admin)]
        public ActionResult Anonymize()
        {
            return View();
        }

        [DenyAccess]
        [AllowAccess(SystemRole.Admin)]
        [HttpPost]
        public ActionResult Anonymize(string vm)
        {
            FileSystem fs = new FileSystem();
            FileCollection fc = fs.File(f => f.Contains("Names"));
            if(fc.Count == 2)
            {
                string[] delimiter = {"\r\n"};
                IDictionary<string, byte[]> fileBits = fc.ToBytes();
                List<string> firstNames = System.Text.Encoding.Default.GetString(fileBits["FirstNames.txt"]).Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();
                List<string> lastNames = System.Text.Encoding.Default.GetString(fileBits["LastNames.txt"]).Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();
                Random rng = new Random();
                List<OsbideUser> users = Db.Users.ToList();
                foreach(OsbideUser user in users)
                {
                    string firstName = firstNames[rng.Next(0, firstNames.Count - 1)];
                    string lastName = lastNames[rng.Next(0, lastNames.Count - 1)];

                    //convert "ADAM" to "Adam"
                    firstName = firstName.First().ToString().ToUpper() + firstName.Substring(1).ToLower();
                    lastName = lastName.First().ToString().ToUpper() + lastName.Substring(1).ToLower();

                    user.FirstName = firstName;
                    user.LastName = lastName;
                }

                Db.SaveChanges();
            }
            return View();
        }


        public ActionResult DailyActivity(int? SelectedStudentId, DateTime? SelectedDate)
        {
            DailyActivityViewModel vm = new DailyActivityViewModel();
            if (SelectedStudentId != null && SelectedDate != null)
            {
                vm.SelectedDate = (DateTime)SelectedDate;
                vm.SelectedStudentId = (int)SelectedStudentId;
                DateTime tomorrow = vm.SelectedDate.AddDays(1);

                //pull event logs
                List<EventLog> logs = Db.EventLogs
                                        .Where(u => u.SenderId == vm.SelectedStudentId)
                                        .Where(e => e.DateReceived > vm.SelectedDate)
                                        .Where(e => e.DateReceived < tomorrow)
                                        .ToList();
                foreach (EventLog log in logs)
                {
                    vm.ActivityItems.Add(log.DateReceived, log);
                }

                //pull request logs from azure storage
                var requestLogs = DomainObjectHelpers.GetAccountRequest(1, vm.SelectedStudentId)
                                        .Where(l => l.CreatorId == vm.SelectedStudentId)
                                        .Where(l => l.AccessDate > vm.SelectedDate)
                                        .Where(l => l.AccessDate < tomorrow)
                                        .ToList();
                foreach (ActionRequestLog log in requestLogs)
                {
                    vm.ActivityItems.Add(log.AccessDate, log);
                }

                //pull comments
                List<LogCommentEvent> comments = Db.LogCommentEvents
                                              .Where(c => c.EventLog.SenderId == vm.SelectedStudentId)
                                              .Where(c => c.EventDate > vm.SelectedDate)
                                              .Where(c => c.EventDate < tomorrow)
                                              .ToList();
                foreach (LogCommentEvent comment in comments)
                {
                    vm.ActivityItems.Add(comment.EventDate, comment);
                }

            }
            vm.Students = Db.Users.Where(u => u.RoleValue == (int)SystemRole.Student).OrderBy(s => s.LastName).ToList();

            return View(vm);
        }

        [HttpPost]
        public ActionResult UploadRoster(HttpPostedFileBase file)
        {
            ViewBag.UploadResult = true;
            List<List<string>> csvList = new List<List<string>>();
            try
            {
                using (CsvReader reader = new CsvReader(file.InputStream))
                {
                    csvList = reader.Parse();
                }
            }
            catch (Exception)
            {
                ViewBag.UploadResult = false;
                return View("Index");
            }

            //Process of creating automatic user subscriptions:
            //1. Group all users by section
            //2. For each student in a given section, assign them to all other students in that section
            Dictionary<int, List<int>> sectionGroups = new Dictionary<int, List<int>>();

            //this loop takes care of process step #1
            foreach (List<string> row in csvList)
            {
                if (row.Count == 2)
                {
                    int studentId = 0;
                    int section = 0;
                    if (Int32.TryParse(row[0], out studentId) == true)
                    {
                        if (Int32.TryParse(row[1], out section) == true)
                        {
                            if (sectionGroups.ContainsKey(section) == false)
                            {
                                sectionGroups.Add(section, new List<int>());
                            }
                            if (sectionGroups[section].Contains(studentId) == false)
                            {
                                sectionGroups[section].Add(studentId);
                            }
                        }
                    }
                }
            }

            //this loop takes care of process step #2
            foreach (int section in sectionGroups.Keys)
            {
                List<int> sectionList = sectionGroups[section];
                List<UserSubscription> dbSubs = (from sub in Db.UserSubscriptions
                                                 where
                                                 (
                                                     sectionList.Contains(sub.ObserverInstitutionId)
                                                     || sectionList.Contains(sub.SubjectInstitutionId)
                                                 )
                                                 && sub.ObserverSchoolId == CurrentUser.SchoolId
                                                 select sub
                                                ).ToList();

                foreach (int observerId in sectionGroups[section])
                {
                    foreach (int subjectId in sectionGroups[section])
                    {
                        if (observerId.CompareTo(subjectId) != 0)
                        {
                            UserSubscription dbSub = dbSubs.Where(s => s.ObserverInstitutionId == observerId).Where(s => s.SubjectInstitutionId == subjectId).FirstOrDefault();
                            if (dbSub == null)
                            {
                                UserSubscription sub = new UserSubscription()
                                {
                                    ObserverInstitutionId = observerId,
                                    SubjectInstitutionId = subjectId,

                                    //assume that the subject and observer attend the same institution as the 
                                    //person uploading the CSV file
                                    ObserverSchoolId = CurrentUser.SchoolId,
                                    SubjectSchoolId = CurrentUser.SchoolId
                                };
                                Db.UserSubscriptions.Add(sub);
                            }
                        }
                    }
                }
            }

            //save any DB changes
            try
            {
                Db.SaveChanges();
            }
            catch (Exception)
            {
                ViewBag.UploadResult = false;
            }

            return View("Index");
        }
    }
}

using Ionic.Zip;
using OSBIDE.Library.Events;
using OSBIDE.Web.Models.Attributes;
using OSBIDE.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    [OsbideAuthorize]
    [NotForStudents]
    public class AssignmentController : ControllerBase
    {
        //
        // GET: /Assignment/

        public ActionResult Index(string assignmentName = "")
        {
            List<string> assignments = Db.SubmitEvents.Select(s => s.AssignmentName).Distinct().ToList();
            assignments.Sort();

            if (string.IsNullOrEmpty(assignmentName) == true)
            {
                assignmentName = assignments.FirstOrDefault();
            }



            //build the view model and return
            AssignmentsViewModel vm = new AssignmentsViewModel();
            vm.AssignmentNames = assignments;
            vm.CurrentAssignmentName = assignmentName;
            vm.Assignments = GetMostRecentSubmissions(assignmentName);

            return View(vm);
        }

        public FileStreamResult Download(string assignmentName)
        {
            List<SubmitEvent> submits = GetMostRecentSubmissions(assignmentName);

            //AC: for some reason, I can't use a USING statement for automatic closure.  Is this 
            //    a potential memory leak?
            MemoryStream finalZipStream = new MemoryStream();
            using (ZipFile finalZipFile = new ZipFile())
            {
                foreach (SubmitEvent submit in submits)
                {
                    using (MemoryStream zipStream = new MemoryStream())
                    {
                        zipStream.Write(submit.SolutionData, 0, submit.SolutionData.Length);
                        zipStream.Position = 0;
                        try
                        {
                            using (ZipFile zip = ZipFile.Read(zipStream))
                            {
                                foreach (ZipEntry entry in zip)
                                {
                                    using (MemoryStream entryStream = new MemoryStream())
                                    {
                                        entry.Extract(entryStream);
                                        entryStream.Position = 0;
                                        string entryName = string.Format("{0}/{1}", submit.EventLog.Sender.FullName, entry.FileName);
                                        finalZipFile.AddEntry(entryName, entryStream.ToArray());
                                    }
                                }
                            }
                        }
                        catch (ZipException)
                        {
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                finalZipFile.Save(finalZipStream);
                finalZipStream.Position = 0;
            }
            return new FileStreamResult(finalZipStream, "application/zip") { FileDownloadName = assignmentName };
        }

        public FileStreamResult DownloadSingle(int id)
        {
            MemoryStream stream = new MemoryStream();
            SubmitEvent submit = Db.SubmitEvents.Where(s => s.EventLogId == id).FirstOrDefault();
            string fileName = "bad download";
            if (submit != null)
            {
                stream.Write(submit.SolutionData, 0, submit.SolutionData.Length);
                stream.Position = 0;
                fileName = string.Format("{0} - {1}", submit.AssignmentName, submit.EventLog.Sender.FullName);
            }
            return new FileStreamResult(stream, "application/zip") { FileDownloadName = fileName };
        }

        private List<SubmitEvent> GetMostRecentSubmissions(string assignmentName)
        {
            //the DB query will get all student submits.  We only want their last submission...
            List<SubmitEvent> submits = Db.SubmitEvents.Include("EventLog").Include("EventLog.Sender").Where(s => s.AssignmentName == assignmentName).OrderBy(s => s.EventDate).ToList();

            //...which we will store into this dictionary
            Dictionary<int, SubmitEvent> lastSubmits = new Dictionary<int, SubmitEvent>();
            foreach (SubmitEvent submit in submits)
            {
                int key = submit.EventLog.SenderId;
                if (lastSubmits.ContainsKey(key) == false)
                {
                    lastSubmits[key] = submit;
                }
                else
                {
                    if (lastSubmits[key].EventDate < submit.EventDate)
                    {
                        lastSubmits[key] = submit;
                    }
                }
            }

            //convert the dictionary back into a list
            submits.Clear();
            foreach (int key in lastSubmits.Keys)
            {
                submits.Add(lastSubmits[key]);
            }

            //order by last name
            submits = submits.OrderBy(s => s.EventLog.Sender.FullName).ToList();
            return submits;
        }

    }
}

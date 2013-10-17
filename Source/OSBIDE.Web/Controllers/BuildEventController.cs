﻿using DiffMatchPatch;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using OSBIDE.Web.Models;
using OSBIDE.Web.Models.Attributes;
using OSBIDE.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    [OsbideAuthorize]
    [RequiresVisualStudioConnectionForStudents]
    public class BuildEventController : ControllerBase
    {
        //
        // GET: /BuildEvent/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Diff(int id, int fileToDiff = -1, string direction = "next")
        {
            BuildEvent original = Db.BuildEvents.Where(b => b.EventLogId == id).FirstOrDefault();
            BuildEvent next = null; //= GetNextEvent(original.EventLogId);

            //Even though we were passed a specific event log id in our parameter list, we really only
            //want to show "interesting" items.  We consider a build event to be interesting if
            // a) it has a build exception
            // b) it happened after a runtime exception (AC Note: currently ignoring exceptions.  Might need a new action for these?)
            if (direction == "next")
            {
                original = (from build in Db.BuildEvents
                            where build.EventLogId >= id
                            && build.ErrorItems.Count > 0
                            && original.EventLog.SenderId == original.EventLog.SenderId
                            && original.SolutionName.CompareTo(original.SolutionName) == 0
                            orderby build.Id ascending
                            select build)
                            .Take(1)
                           .FirstOrDefault();
                if (original != null)
                {
                    next = GetNextEvent(original.EventLogId);
                }
            }
            else
            {
                original = (from build in Db.BuildEvents
                            where build.EventLogId <= id
                            && build.ErrorItems.Count > 0
                            && original.EventLog.SenderId == original.EventLog.SenderId
                            && original.SolutionName.CompareTo(original.SolutionName) == 0
                            orderby build.Id descending
                            select build)
                            .Take(1)
                           .FirstOrDefault();
                if (original != null)
                {
                    next = GetNextEvent(original.EventLogId);
                }
            }
            //just in case we messed up in the previous if/else block
            if (original == null)
            {
                original = Db.BuildEvents.Where(b => b.EventLogId == id).FirstOrDefault();
                next = GetNextEvent(original.EventLogId);
            }

            //also look for exception events if applicable
            ExceptionEvent originalEx = null;
            if (next != null)
            {
                originalEx = GetBuildException(original.EventLogId, next.EventLogId);
            }
            else
            {
                next = new BuildEvent();
                next.EventLogId = -1;
                next.Id = -1;
                next.EventLog = new EventLog();
                next.EventLog.DateReceived = original.EventLog.DateReceived;
            }

            //sanity check before we begin processing
            if (original == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.PreviousLogId = GetPreviousDiffId(original.EventLogId);
            ViewBag.OriginalLog = original;
            ViewBag.NextLog = next;

            //build our view model
            BuildDiffViewModel vm = BuildViewModel(original, next, originalEx);

            //check for a requested file name.  If none exists, set to the first file listed (alphabetically)
            if (fileToDiff > 0)
            {
                BuildDocument activeDoc = original.Documents.Where(d => d.DocumentId == fileToDiff).FirstOrDefault();
                if (activeDoc != null)
                {
                    vm.ActiveFileId = fileToDiff;
                }
            }

            //no file was specified, try something else
            if (vm.ActiveFileId == 0)
            {
                //start with null document
                vm.ActiveFileId = -1;

                //and try to find a document that has errors
                foreach (BuildEventErrorListItem criticalError in original.CriticalErrorItems)
                {
                    string errorFileName = Path.GetFileName(criticalError.ErrorListItem.File).ToLower();
                    foreach (BuildDocument doc in original.Documents)
                    {
                        string docFileName = Path.GetFileName(doc.Document.FileName);
                        if (errorFileName.CompareTo(docFileName) == 0)
                        {
                            vm.ActiveFileId = doc.DocumentId;
                            break;
                        }
                    }
                }
            }
            return View(vm);
        }

        /// <summary>
        /// Returns the build that follows the supplied log Id for the given user
        /// </summary>
        /// <param name="buildLogId"></param>
        /// <param name="userId"></param>
        /// <returns>-1 if nothing was found, the LogId of the associated build otherwise</returns>
        public int GetNextDiffId(int id)
        {
            BuildEvent next = GetNextEvent(id);
            if (next == null)
            {
                return -1;
            }
            return next.EventLogId;
        }

        public int GetPreviousDiffId(int id)
        {
            BuildEvent original = Db.BuildEvents.Where(b => b.EventLogId == id).FirstOrDefault();
            if (original == null)
            {
                return -1;
            }

            //find the next build in the sequence that:
            // a: belongs to the same user
            // b: is from the same solution
            BuildEvent previous = (from be in Db.BuildEvents
                                   where be.EventLogId < id
                                   && be.EventLog.SenderId == original.EventLog.SenderId
                                   && be.SolutionName.CompareTo(original.SolutionName) == 0
                                   orderby be.EventLogId descending
                                   select be).FirstOrDefault();
            if (previous != null)
            {
                return previous.EventLogId;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// (AJAX) Returns a diff between two builds
        /// </summary>
        /// <param name="firstLogId"></param>
        /// <param name="secondLogId"></param>
        /// <returns></returns>
        public ActionResult GetDiff(int id, string direction = "next")
        {
            BuildEvent original = Db.BuildEvents.Where(b => b.EventLogId == id).FirstOrDefault();
            if (direction == "next")
            {
                while (original != null && original.CriticalErrorCount == 0)
                {
                    original = GetNextEvent(original.EventLogId);
                }
            }
            else
            {
                while (original != null && original.CriticalErrorCount == 0)
                {
                    int previousId = GetPreviousDiffId(original.EventLogId);
                    original = Db.BuildEvents.Where(b => b.EventLogId == previousId).FirstOrDefault();
                }
            }
            if (original == null)
            {
                original = Db.BuildEvents.Where(b => b.EventLogId == id).FirstOrDefault();
            }
            BuildEvent next = GetNextEvent(original.EventLogId);
            ExceptionEvent originalEx = null;
            if (next != null)
            {
                GetBuildException(original.EventLogId, next.EventLogId);
            }
            else
            {
                next = new BuildEvent();
                next.EventLogId = -1;
                next.Id = -1;
                next.EventLog = new EventLog();
                next.EventLog.DateReceived = original.EventLog.DateReceived;
            }
            if (original == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.OriginalLog = original;
            ViewBag.NextLog = next;
            BuildDiffViewModel vm = BuildViewModel(original, next, originalEx);
            return View(vm);
        }

        /// <summary>
        /// Will locate any exception that occurred between the two supplied event log ids
        /// </summary>
        /// <param name="buildId"></param>
        /// <param name="nextBuildId"></param>
        /// <returns></returns>
        public ExceptionEvent GetBuildException(int logId, int nextLogId)
        {
            ExceptionEvent ex = Db.ExceptionEvents.Where(e => e.EventLogId > logId).Where(e => e.EventLogId < nextLogId).FirstOrDefault();
            return ex;
        }

        private BuildEvent GetNextEvent(int id)
        {
            BuildEvent original = Db.BuildEvents.Where(b => b.EventLogId == id).FirstOrDefault();
            if (original == null)
            {
                return null;
            }

            //find the next build in the sequence that:
            // a: belongs to the same user
            // b: is from the same solution
            BuildEvent next = Db.BuildEvents
                .Where(b => b.EventLogId > id)
                .Where(b => b.EventLog.SenderId == original.EventLog.SenderId)
                .Where(b => b.SolutionName.CompareTo(original.SolutionName) == 0)
                .FirstOrDefault();
            return next;
        }

        private BuildDiffViewModel BuildViewModel(BuildEvent first, BuildEvent second, ExceptionEvent firstBuildException = null)
        {
            BuildDiffViewModel vm = new BuildDiffViewModel();
            diff_match_patch diff = new diff_match_patch();

            //loop through each document in the first, original build
            foreach (BuildDocument doc in first.Documents)
            {
                CodeDocument firstDoc = doc.Document;
                CodeDocument secondDoc = second.Documents.Where(d => d.Document.FileName == doc.Document.FileName).Select(d => d.Document).FirstOrDefault();
                if (secondDoc == null)
                {
                    continue;
                }

                //compute the diff
                List<Diff> diffs = diff.diff_lineMode(firstDoc.Content, secondDoc.Content);
                StringBuilder firstBuilder = new StringBuilder();
                StringBuilder secondBuilder = new StringBuilder();

                //loop through each piece in the return list of diffs.  Three possibilities:
                // EQUAL: the documents share the same code, so re-include in both
                // DELETE: The code existed in the original but not in the modified.  Add back to the original (first).
                // INSERT: The code exists in the modified, but not the original.  Add back to the modified (second).
                foreach (Diff d in diffs)
                {
                    switch (d.operation)
                    {
                        case Operation.DELETE:

                            //encase the deleted code in a special token so that we can find it more easily later
                            firstBuilder.Append(string.Format("{0}{1}", BuildDiffViewModel.DIFF_ESCAPE, d.text));
                            break;

                        case Operation.EQUAL:
                        default:
                            firstBuilder.Append(d.text);
                            secondBuilder.Append(d.text);
                            break;

                        case Operation.INSERT:

                            //encase the inserted code in a special token so that we can find it more easily later
                            secondBuilder.Append(string.Format("{0}{1}", BuildDiffViewModel.DIFF_ESCAPE, d.text));
                            break;
                    }
                }
                firstDoc.Content = firstBuilder.ToString();
                secondDoc.Content = secondBuilder.ToString();
            }

            //figure out what lines had build errors on them.
            foreach (BuildEventErrorListItem item in first.CriticalErrorItems)
            {
                string fileName = Path.GetFileName(item.ErrorListItem.File);
                DocumentError error = new DocumentError()
                {
                    FileName = fileName,
                    LineNumber = item.ErrorListItem.Line - 1, //adjust line number by 1 because c# arrays are 0-based
                    Source = DocumentErrorSource.Build,
                    ErrorMessage = item.ErrorListItem.Description
                };
                vm.RecordError(error);
            }

            //now find the line on which any runtime exception occurred on
            if (firstBuildException != null)
            {
                string fileName = Path.GetFileName(firstBuildException.DocumentName);
                DocumentError error = new DocumentError()
                {
                    FileName = fileName,
                    LineNumber = firstBuildException.LineNumber - 1,  //adjust line number by 1 because c# arrays are 0-based
                    Source = DocumentErrorSource.Debug,
                    ErrorMessage = firstBuildException.ExceptionDescription
                };
                vm.RecordError(error);
            }

            vm.OriginalBuild = first;
            vm.ModifiedBuild = second;
            return vm;
        }

    }
}

using OSBIDE.Analytics.Library.Models;
using OSBIDE.Analytics.Web.Models.ViewModels.CommentAnalyzer;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using OSBIDE.Analytics.Web.ViewModels;

namespace OSBIDE.Analytics.Web.Controllers
{
    public class CommentAnalyzerController : ControllerBase
    {
        /*Pages:
         * Index: Show list of students by class.  Include Post count & document save count.
         * Student: Shows all comments by student.  Also includes time of previous and next edit
         * Details: Shows comment, code before, and code after
         * */

        // GET: CommentAnalyzer
        public ActionResult Index(int courseId = 3)
        {
            //grab all students in the course
            var studentsQuery = from student in OsbideDb.Users
                                join cur in OsbideDb.CourseUserRelationships on student.Id equals cur.UserId
                                join course in OsbideDb.Courses on cur.CourseId equals course.Id
                                where course.Id == courseId
                                select new { Student = student, Course = course };
            var students = studentsQuery.ToList();

            //grab num posts by student
            var numPosts = from item in
                               (from log in OsbideDb.EventLogs
                                where log.LogType == "FeedPostEvent"
                                select log
                                )
                           group item by item.SenderId into items
                           select new { StudentId = items.Key, Count = items.Count() };
            Dictionary<int, int> postsByStudent = new Dictionary<int, int>();
            foreach (var item in numPosts)
            {
                postsByStudent.Add(item.StudentId, item.Count);
            }

            //grab num replies by student
            var numReplies = from item in
                                 (from log in OsbideDb.EventLogs
                                  where log.LogType == "LogCommentEvent"
                                  select log
                                  )
                             group item by item.SenderId into items
                             select new { StudentId = items.Key, Count = items.Count() };
            Dictionary<int, int> repliesByStudent = new Dictionary<int, int>();
            foreach (var item in numReplies)
            {
                repliesByStudent.Add(item.StudentId, item.Count);
            }

            //grab number of saves by student
            var numSaves = from item in
                               (from log in OsbideDb.EventLogs
                                where log.LogType == "SaveEvent"
                                select log
                                )
                           group item by item.SenderId into items
                           select new { StudentId = items.Key, Count = items.Count() };
            Dictionary<int, int> savesByStudent = new Dictionary<int, int>();
            foreach (var item in numSaves)
            {
                savesByStudent.Add(item.StudentId, item.Count);
            }

            IndexViewModel vm = new IndexViewModel();
            vm.Course = students.FirstOrDefault().Course;
            vm.Users = students.Select(s => s.Student).OrderBy(s => s.LastName).ToList();
            vm.PostsByUser = postsByStudent;
            vm.RepliesByUser = repliesByStudent;
            vm.SavesByUser = savesByStudent;
            return View(vm);
        }

        /// <summary>
        /// Gets comment details for a particular student
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Student(int id)
        {
            /*
             * to fetch:
             * For each comment:
             *      Comment
             *      Option to view entire thread
             *      Chris, Adam, Carla content coding info
             *      Adam's student EC coding info
             *      size of previous / next save
             *      time of prevous /  last save
             *      NPSM state
             * */
            List<StudentCommentTimeline> viewModel = new List<StudentCommentTimeline>();

            //id is nice for MVC, but not very descriptive, switch to studentId in body.
            int studentId = id;
            int[] interestingEventIds =
            {
                  1  //ask for help 
                , 2  //build event
                , 7  //feed post
                , 9  //log comment
                , 10 //save
            };

            //this query pulls most questions (ask for help excluded?) from the analytics DB
            //and should be faster than loading all questions from the OSBIDE DB
            var commentQuery = from comment in Db.Posts
                               where comment.AuthorId == studentId
                               select comment;

            //convert into dictionary for lookup by logsQuery
            Dictionary<int, Post> posts = new Dictionary<int, Post>();
            foreach (Post post in commentQuery)
            {
                posts.Add(post.OsbideId, post);
            }

            //This query will pull down all content coding questions, organized by Osbide user ID
            var contentCodingQuery = from code in Db.ContentCodings
                                     where code.AuthorId == studentId
                                     select code;
            SortedList<DateTime, ContentCoding> expertCodings = new SortedList<DateTime, ContentCoding>();
            foreach (ContentCoding coding in contentCodingQuery)
            {
                //I was getting key mismatch (probably difference in milliseconds).  My solution was to create
                //a new date using only coarser measures
                DateTime dateKey = new DateTime(coding.Date.Year, coding.Date.Month, coding.Date.Day, coding.Date.Hour, coding.Date.Minute, coding.Date.Second, DateTimeKind.Utc);
                expertCodings.Add(dateKey, coding);
            }

            //This query will pull down information obtained from my crowd-sourced content coding.
            //AnswerCodings have FK reference to the original question as well as the answer.  If a Post
            //is in the AnswerCodings table, it must be an answer
            var answeredQuestionsQuery = from answer in Db.AnswerCodings
                                       .Include(c => c.Answer)
                                       .Include(c => c.Question)
                                         where answer.Answer.AuthorId == studentId || answer.Question.AuthorId == studentId
                                         select answer;

            var allPostsQuery = from question in Db.QuestionCodings
                                    .Include(c => c.Post)
                                where question.Post.AuthorId == studentId
                                select question;
            Dictionary<int, PostCoding> crowdCodings = new Dictionary<int, PostCoding>();
            foreach (QuestionCoding coding in allPostsQuery)
            {
                if (crowdCodings.ContainsKey(coding.Post.OsbideId) == false)
                {
                    crowdCodings.Add(coding.Post.OsbideId, new PostCoding());
                    crowdCodings[coding.Post.OsbideId].OsbidePostId = coding.Post.OsbideId;
                }
                crowdCodings[coding.Post.OsbideId].Codings.Add(coding);
            }
            foreach (AnswerCoding coding in answeredQuestionsQuery)
            {
                if (crowdCodings.ContainsKey(coding.Question.OsbideId) == false)
                {
                    crowdCodings.Add(coding.Question.OsbideId, new PostCoding());
                    crowdCodings[coding.Question.OsbideId].OsbidePostId = coding.Question.OsbideId;
                }
                crowdCodings[coding.Question.OsbideId].Responses.Add(coding);
            }

            //grab all save and build events for this user
            Dictionary<int, SaveEvent> allSaves = new Dictionary<int, SaveEvent>();
            Dictionary<int, BuildEvent> allBuilds = new Dictionary<int, BuildEvent>();
            var savesQuery = from save in OsbideDb.SaveEvents
                             .Include(s => s.Document)
                             join log in OsbideDb.EventLogs on save.EventLogId equals log.Id
                             where log.SenderId == studentId
                             select save;
            foreach (SaveEvent saveEvent in savesQuery)
            {
                allSaves[saveEvent.EventLogId] = saveEvent;
            }
            var buildsQuery = from build in OsbideDb.BuildEvents
                             .Include(b => b.Documents.Select(d => d.Document))
                              join log in OsbideDb.EventLogs on build.EventLogId equals log.Id
                              where log.SenderId == studentId
                              select build;
            foreach (BuildEvent buildEvent in buildsQuery)
            {
                allBuilds[buildEvent.EventLogId] = buildEvent;
            }

            //this query pulls data directly from event logs.
            var logsQuery = from log in OsbideDb.EventLogs
                            where log.SenderId == studentId && interestingEventIds.Contains(log.EventTypeId)
                            select log;
            List<EventLog> eventLogs = logsQuery.ToList();

            Stack<EventLog> saveEvents = new Stack<EventLog>();
            List<EventLog> socialEvents = new List<EventLog>();

            foreach (EventLog log in eventLogs)
            {
                //holds the next entry into the view model
                StudentCommentTimeline nextViewModel = new StudentCommentTimeline();

                //if we have a document save event, remember for later until we get a social event
                if (log.LogType == SaveEvent.Name || log.LogType == BuildEvent.Name)
                {
                    saveEvents.Push(log);
                }
                else
                {
                    //social event detected

                    //1: grab previous edit information
                    string solutionName = "";
                    Dictionary<string, CodeDocument> previousDocuments = new Dictionary<string, CodeDocument>();

                    //Start with saves as they will contain more up-to-date information than last build
                    while (saveEvents.Count > 0 && saveEvents.Peek().LogType != BuildEvent.Name)
                    {
                        EventLog next = saveEvents.Pop();
                        if(allSaves.ContainsKey(next.Id))
                        {
                            SaveEvent save = allSaves[next.Id];
                            if (solutionName.Length == 0)
                            {
                                solutionName = save.SolutionName;
                            }
                            if (save.SolutionName == solutionName)
                            {
                                if (previousDocuments.ContainsKey(save.Document.FileName) == false)
                                {
                                    previousDocuments[save.Document.FileName] = save.Document;
                                }
                            }
                        }
                    }

                    //at this point, saveEvents should be empty or we should be at a build event.
                    //Finish off the snapshot with documents transferred with last build
                    if (saveEvents.Count > 0)
                    {
                        EventLog top = saveEvents.Pop();
                        if(allBuilds.ContainsKey(top.Id))
                        {
                            BuildEvent build = allBuilds[top.Id];

                            if (solutionName.Length == 0)
                            {
                                solutionName = build.SolutionName;
                            }
                            if (build.SolutionName == solutionName)
                            {
                                foreach (BuildDocument doc in build.Documents)
                                {
                                    if (previousDocuments.ContainsKey(doc.Document.FileName) == false)
                                    {
                                        previousDocuments[doc.Document.FileName] = doc.Document;
                                    }
                                }
                            }
                        }
                    }


                    //store final result in view model
                    nextViewModel.CodeBeforeComment = previousDocuments;

                    //2: grab next edit information (will have to be done on 2nd pass)

                    //grab expert content coding info
                    DateTime dateKey = new DateTime(log.DateReceived.Year,
                                                    log.DateReceived.Month,
                                                    log.DateReceived.Day,
                                                    log.DateReceived.Hour,
                                                    log.DateReceived.Minute,
                                                    log.DateReceived.Second,
                                                    DateTimeKind.Utc);
                    //I was getting key mismatch (probably difference in milliseconds).  My solution was to create
                    //a new date using only coarser measures
                    if (expertCodings.ContainsKey(dateKey))
                    {
                        nextViewModel.ExpertCoding = expertCodings[dateKey];
                    }

                    //grab crowd coding info
                    var crowd = crowdCodings.Where(cc => cc.Key == log.Id).Select(k => k.Value).FirstOrDefault();
                    if(crowd != null)
                    {
                        nextViewModel.CrowdCodings = crowd;
                    }

                    //grab NPSM state info
                    var npsmQuery = from npsm in Db.TimelineStates
                                    where npsm.EndTime <= log.DateReceived && npsm.IsSocialEvent == false
                                    orderby npsm.Id descending
                                    select npsm;
                    TimelineState state = npsmQuery.Take(1).FirstOrDefault();
                    if (state != null)
                    {
                        nextViewModel.ProgrammingState = state;
                    }


                    //add in comment information
                    if (posts.ContainsKey(log.Id) == true)
                    {
                        nextViewModel.Comment = posts[log.Id].Content;
                    }
                    else
                    {
                        //not found in pre-query.  Pull manually
                        if (log.LogType == FeedPostEvent.Name)
                        {
                            FeedPostEvent feedPost = OsbideDb.FeedPostEvents.Where(fpe => fpe.EventLogId == log.Id).FirstOrDefault();
                            if (feedPost != null)
                            {
                                nextViewModel.Comment = feedPost.Comment;
                            }
                        }
                        else if (log.LogType == LogCommentEvent.Name)
                        {
                            LogCommentEvent logComment = OsbideDb.LogCommentEvents.Where(fpe => fpe.EventLogId == log.Id).FirstOrDefault();
                            if (logComment != null)
                            {
                                nextViewModel.Comment = logComment.Content;
                            }
                        }
                        else if (log.LogType == AskForHelpEvent.Name)
                        {
                            AskForHelpEvent ask = OsbideDb.AskForHelpEvents.Where(fpe => fpe.EventLogId == log.Id).FirstOrDefault();
                            if (ask != null)
                            {
                                nextViewModel.Comment = ask.UserComment + "\n" + ask.Code;
                            }
                        }
                    }
                    nextViewModel.Log = log;
                    nextViewModel.Author = log.Sender;
                    viewModel.Add(nextViewModel);
                }
            }

            //2nd pass: find code modifications made after comment.  
            for (int i = 0; i < viewModel.Count; i++)
            {
                StudentCommentTimeline current = viewModel[i];
                StudentCommentTimeline next = new StudentCommentTimeline();
                if(i + 1 < viewModel.Count)
                {
                    next = viewModel[i + 1];
                }
                else
                {
                    next.Log = eventLogs.Last();
                }

                List<EventLog> logsBetween = eventLogs
                    .Where(l => l.DateReceived >= current.Log.DateReceived)
                    .Where(l => l.DateReceived <= next.Log.DateReceived)
                    .ToList();

                Dictionary<string, CodeDocument> nextDocuments = new Dictionary<string, CodeDocument>();
                string solutionName = "";
                foreach (EventLog log in logsBetween)
                {
                    if (log.LogType == SaveEvent.Name)
                    {
                        if(allSaves.ContainsKey(log.Id))
                        {
                            SaveEvent save = allSaves[log.Id];
                            if (solutionName.Length == 0)
                            {
                                solutionName = save.SolutionName;
                            }
                            if (save.SolutionName == solutionName)
                            {
                                if (nextDocuments.ContainsKey(save.Document.FileName) == false)
                                {
                                    nextDocuments[save.Document.FileName] = save.Document;
                                }
                            }
                        }
                    }
                    else if (log.LogType == BuildEvent.Name)
                    {
                        if(allBuilds.ContainsKey(log.Id))
                        {
                            BuildEvent build = allBuilds[log.Id];
                            if (solutionName.Length == 0)
                            {
                                solutionName = build.SolutionName;
                            }
                            if (build.SolutionName == solutionName)
                            {
                                foreach (BuildDocument doc in build.Documents)
                                {
                                    if (nextDocuments.ContainsKey(doc.Document.FileName) == false)
                                    {
                                        nextDocuments[doc.Document.FileName] = doc.Document;
                                    }
                                }
                            }
                        }
                    }
                }
                current.CodeAfterComment = nextDocuments;
            }

            return View(viewModel);
        }
    }
}
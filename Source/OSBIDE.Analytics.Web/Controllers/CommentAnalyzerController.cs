using OSBIDE.Analytics.Web.Models.ViewModels.CommentAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
            foreach(var item in numPosts)
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
    }
}
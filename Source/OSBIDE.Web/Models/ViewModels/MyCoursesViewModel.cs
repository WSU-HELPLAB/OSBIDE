using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models.ViewModels
{
    public class MyCoursesViewModel
    {
        public OsbideUser CurrentUser { get; set; }

        /// <summary>
        /// A list of all courses in which the current user is a course coordiantor
        /// </summary>
        public List<CourseCoordinator> CoordinatingCourses { get; set; }

        /// <summary>
        /// A list of all courses in which the current user is an assistant
        /// </summary>
        public List<CourseAssistant> AssistingCourses { get; set; }

        /// <summary>
        /// A list of all courses in which the current users is a student
        /// </summary>
        public List<CourseStudent> StudentCourses { get; set; }

        /// <summary>
        /// A list of all courses currently in OSBIDE
        /// </summary>
        public List<Course> AllCourses { get; set; }

        public MyCoursesViewModel()
        {
            CoordinatingCourses = new List<CourseCoordinator>();
            AssistingCourses = new List<CourseAssistant>();
            StudentCourses = new List<CourseStudent>();
            AllCourses = new List<Course>();
            CurrentUser = new OsbideUser();
        }
    }
}
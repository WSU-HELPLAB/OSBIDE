using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models.ViewModels
{
    public class CoursesViewModel
    {
        public List<Course> AllCourses { get; set; }
        public Dictionary<string, SortedDictionary<string, Course>> CoursesByPrefix { get; set; }
        public OsbideUser CurrentUser { get; set; }

        public CoursesViewModel()
        {
            AllCourses = new List<Course>();
            CoursesByPrefix = new Dictionary<string, SortedDictionary<string, Course>>();
        }
    }
}
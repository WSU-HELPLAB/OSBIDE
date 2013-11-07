using OSBIDE.Library.Models;
using OSBIDE.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    public class CourseController : ControllerBase
    {
        //
        // GET: /Courses/

        public ActionResult Index()
        {
            return RedirectToAction("MyCourses");
        }

        public ActionResult MyCourses()
        {
            return View();
        }

        public ActionResult Search()
        {
            CoursesViewModel vm = new CoursesViewModel();
            vm.AllCourses = Db.Courses
                .Where(c => c.SchoolId == CurrentUser.SchoolId)
                .OrderBy(c => c.Prefix)
                .ToList();
            vm.CurrentUser = CurrentUser;

            foreach (Course course in vm.AllCourses)
            {
                if (vm.CoursesByPrefix.ContainsKey(course.Prefix) == false)
                {
                    vm.CoursesByPrefix.Add(course.Prefix, new SortedDictionary<string,Course>());
                }
                vm.CoursesByPrefix[course.Prefix].Add(course.CourseNumber, course);
            }
            return View(vm);
        }

    }
}

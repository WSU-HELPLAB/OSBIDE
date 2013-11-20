using OSBIDE.Library.Models;
using OSBIDE.Web.Models.Attributes;
using OSBIDE.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    [OsbideAuthorize]
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

        public ActionResult Details(int id = -1)
        {
            return View();
        }

        public JsonResult GetAllCourses()
        {
            CoursesViewModel vm = BuildViewModel();
            var simpleCourse = vm.CoursesByPrefix.Select(c => new
            {
                Prefix = c.Key,
                Courses = c.Value.Select(
                    co => new
                    {
                        Prefix = c.Key,
                        CourseNumber = co.Value.CourseNumber,
                        Description = co.Value.Description
                    }
                )
            });
            return this.Json(simpleCourse, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Search()
        {
            CoursesViewModel vm = BuildViewModel();            
            return View(vm);
        }

        [HttpPost]
        public ActionResult Search(CoursesViewModel vm)
        {
            vm = BuildViewModel(vm);
            if(vm.SelectedCourse > 0)
            {
                Course toJoin = Db.Courses.Where(c => c.Id == vm.SelectedCourse).FirstOrDefault();
                if (toJoin != null)
                {
                    CourseUserRelationship cur = new CourseUserRelationship()
                    {
                        CourseId = toJoin.Id,
                        UserId = CurrentUser.Id,
                        Role = CourseRole.Student,
                        IsApproved = !toJoin.RequiresApprovalBeforeAdmission,
                        IsActive = true
                    };
                    Db.CourseUserRelationships.Add(cur);
                    Db.SaveChanges();

                    if (toJoin.RequiresApprovalBeforeAdmission == true)
                    {
                        vm.ServerMessage = string.Format("A request to join {0} has been sent to the course instructor.  Until then, certain features related to the course may be unavailable.", toJoin.Name);
                    }
                    else
                    {
                        vm.ServerMessage = string.Format("You are now enrolled in {0}.", toJoin.Name);
                    }
                }
                else
                {
                    vm.ServerMessage = "There server experienced an error when trying to add you to the selected course.  Please try again.  If the problem persists, please contact support@osbide.com.";
                }
            }
            return View(vm);
        }

        private CoursesViewModel BuildViewModel(CoursesViewModel vm = null)
        {
            if(vm == null)
            {
                vm = new CoursesViewModel();
            }
            vm.AllCourses = Db.Courses
                .Where(c => c.SchoolId == CurrentUser.SchoolId)
                .OrderBy(c => c.Prefix)
                .ToList();
            vm.CurrentUser = CurrentUser;

            foreach (Course course in vm.AllCourses)
            {
                if (vm.CoursesByPrefix.ContainsKey(course.Prefix) == false)
                {
                    vm.CoursesByPrefix.Add(course.Prefix, new SortedDictionary<string, Course>());
                }
                vm.CoursesByPrefix[course.Prefix].Add(course.CourseNumber, course);
            }
            return vm;
        }

    }
}

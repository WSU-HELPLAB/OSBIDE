using OSBIDE.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    public class CoursesController : ControllerBase
    {
        //
        // GET: /Courses/

        public ActionResult Index()
        {
            return RedirectToAction("MyCourses");
        }

        public ActionResult MyCourses()
        {
            MyCoursesViewModel vm = new MyCoursesViewModel();
            vm.CurrentUser = CurrentUser;
            vm.AllCourses = Db.Courses.ToList();
            vm.AssistingCourses = Db.CourseAssistants.Where(c => c.AssistantId == CurrentUser.Id).ToList();
            vm.CoordinatingCourses = Db.CourseCoordinators.Where(c => c.CoordinatorId == CurrentUser.Id).ToList();
            vm.StudentCourses = Db.CourseStudents.Where(c => c.StudentId == CurrentUser.Id).ToList();
            return View(vm);   
        }

    }
}

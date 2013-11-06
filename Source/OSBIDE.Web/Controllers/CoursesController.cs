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
            return View();
        }

    }
}

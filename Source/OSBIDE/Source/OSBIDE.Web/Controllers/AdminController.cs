using OSBIDE.Library.Models;
using OSBIDE.Web.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    [AllowAccess(SystemRole.Instructor)]
    public class AdminController : Controller
    {
        //
        // GET: /Admin/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadRoster(HttpPostedFileBase file)
        {
            ViewBag.UploadResult = true;
            return View("Index");
        }
    }
}

using DiffMatchPatch;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using OSBIDE.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    public class HomeController : ControllerBase
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public int PostComment(string logId, string comment, string returnUrl)
        {
            int id = -1;
            if (Int32.TryParse(logId, out id) == true)
            {
                LogComment logComment = new LogComment()
                {
                    AuthorId = CurrentUser.Id,
                    Content = comment,
                    LogId = id
                };

                Db.LogComments.Add(logComment);
                Db.SaveChanges();
            }

            Response.Redirect(returnUrl);
            return id;
        }
    }
}

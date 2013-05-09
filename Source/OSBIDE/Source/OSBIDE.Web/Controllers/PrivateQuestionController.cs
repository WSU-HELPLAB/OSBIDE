using OSBIDE.Library.Models;
using OSBIDE.Web.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    [OsbideAuthorize]
    public class PrivateQuestionController : ControllerBase
    {
        public ActionResult Index()
        {
            ViewBag.QuestionSubmitted = false;
            PrivateQuestion question = new PrivateQuestion();
            question.UserId = CurrentUser.Id;
            return View(question);
        }

        [HttpPost]
        public ActionResult Index(PrivateQuestion model)
        {
            model.SubmissionDate = DateTime.Now;
            try
            {
                Db.PrivateQuestions.Add(model);
                Db.SaveChanges();
            }
            catch (Exception)
            {
            }
            model.Question = "";
            ViewBag.QuestionSubmitted = true;
            return View(model);
        }
    }
}

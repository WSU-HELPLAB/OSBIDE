using System.Web.Mvc;

using OSBIDE.Library.Models;
using OSBIDE.Web.Models.Attributes;

namespace OSBIDE.Web.Controllers
{
    [AllowAccess(SystemRole.Instructor, SystemRole.Admin)]
    public class CalendarVisualizationController : ControllerBase
    {
        public ActionResult Index()
        {
            return View("~/Views/Analytics/CalendarVisualization.cshtml");
        }
    }
}
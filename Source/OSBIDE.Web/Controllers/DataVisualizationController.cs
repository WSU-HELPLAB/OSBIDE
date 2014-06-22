using System.Web.Mvc;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase;

namespace OSBIDE.Web.Controllers
{
    public class DataVisualizationController : ControllerBase
    {
        //
        // GET: /DataVisualization/
        public ActionResult Index()
        {
            return View("~/Views/Analytics/DataVisualization.cshtml");
        }

        public ActionResult GetData(int timeScale, int? timeout)
        {
            return Json(TimelineChartDataProc.Get((TimeScale)timeScale, timeout), JsonRequestBehavior.AllowGet);
        }
    }
}
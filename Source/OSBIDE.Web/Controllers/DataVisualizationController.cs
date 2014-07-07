using System;
using System.Web.Mvc;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase;
using OSBIDE.Web.Models.Analytics;

namespace OSBIDE.Web.Controllers
{
    public class DataVisualizationController : ControllerBase
    {
        //
        // GET: /DataVisualization/
        public ActionResult Index()
        {
            var analytics = Analytics.FromSession();
            if (analytics.VisualizationParams == null)
            {
                analytics.VisualizationParams = new VisualizationParams();
            }

            // always start from criteria data entry
            analytics.VisualizationParams.TimeFrom = analytics.Criteria.DateFrom;
            analytics.VisualizationParams.TimeTo = analytics.Criteria.DateTo;

            return View("~/Views/Analytics/DataVisualization.cshtml", analytics.VisualizationParams);
        }

        public ActionResult GetData(int? timeScale, DateTime? timeFrom, DateTime? timeTo, int? timeout, bool grayscale)
        {
            var analytics = Analytics.FromSession();
            analytics.VisualizationParams.TimeFrom = timeFrom;
            analytics.VisualizationParams.TimeTo = timeTo;
            analytics.VisualizationParams.TimeScale = (TimeScale)timeScale;
            analytics.VisualizationParams.Timeout = timeout;
            analytics.VisualizationParams.GrayScale = grayscale;

            var chartData = TimelineChartDataProc.Get(timeFrom, timeTo, analytics.SelectedDataItems, (TimeScale)timeScale, timeout, grayscale);
            return Json(chartData, JsonRequestBehavior.AllowGet);
        }
    }
}

using System;
using System.Web.Mvc;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase;
using OSBIDE.Web.Models.Analytics;
using System.Text;

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

        public ActionResult GetCSVData(int? scaleSetting, DateTime? timeFrom, DateTime? timeTo, int? timeout, bool? grayscale)
        {
            var analytics = Analytics.FromSession();
            analytics.VisualizationParams.TimeFrom = timeFrom;
            analytics.VisualizationParams.TimeTo = timeTo;
            analytics.VisualizationParams.TimeScale = (TimeScale)scaleSetting;
            analytics.VisualizationParams.Timeout = timeout;
            analytics.VisualizationParams.GrayScale = grayscale.HasValue ? grayscale.Value : false;

            var chartCsvData = TimelineChartDataProc.GetCSV(timeFrom, timeTo, analytics.SelectedDataItems, (TimeScale)scaleSetting, timeout, analytics.VisualizationParams.GrayScale);
            var csv = File(new System.Text.UTF8Encoding().GetBytes(chartCsvData), "text/csv", "timeline.csv");

            return csv;
        }

        public ActionResult ProcessAzureTableStorage()
        {
            return Json(PassiveSocialEventUtilProc.Run(CurrentUser.SchoolId), JsonRequestBehavior.AllowGet);
        }

        private static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}

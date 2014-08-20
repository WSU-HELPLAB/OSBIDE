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

            var chartData = TimelineChartDataProc.Get(timeFrom, timeTo, analytics.SelectedDataItems, (TimeScale)scaleSetting, timeout, analytics.VisualizationParams.GrayScale);

            var csvText = new StringBuilder();
            chartData.ForEach(x =>
            {
                csvText.Append(x.title);

                var i = 0;
                var j = 0;
                while (i < x.measures.Count || j < x.markers.Count)
                {
                    if (i >= x.measures.Count)
                    {
                        csvText.AppendFormat(",{0},{1}", x.markers[j].Name , x.markers[j].EventTimeDisplayText);
                        j++;
                    }
                    else if (j >= x.markers.Count)
                    {
                        csvText.AppendFormat(",{0},{1},{2}", x.measures[i].Name, x.measures[i].StartTimeDisplayText, x.measures[i].EndTimeDisplayText);
                        i++;
                    }
                    else
                    {
                        if (x.markers[j].Position < x.measures[i].StartPoint)
                        {
                            csvText.AppendFormat(",{0},{1}", x.markers[j].Name, x.markers[j].EventTimeDisplayText);
                            j++;
                        }
                        else
                        {
                            csvText.AppendFormat(",{0},{1},{2}", x.measures[i].Name, x.measures[i].StartTimeDisplayText, x.measures[i].EndTimeDisplayText);
                            i++;
                        }
                    }
                }
                csvText.AppendFormat("{0}", Environment.NewLine);
            });
            var csv = File(new System.Text.UTF8Encoding().GetBytes(csvText.ToString()), "text/csv", "timeline.csv");
            return csv;
        }

        public ActionResult ProcessAzureTableStorage()
        {
            return Json(PassiveSocialEventUtilProc.ProcessAzureTableStorage(1/*CurrentUser.SchoolId*/), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProcessSQLTableStorage()
        {
            return Json(PassiveSocialEventUtilProc.ProcessSQLTableStorage(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSQL()
        {
            return File(GetBytes(PassiveSocialEventUtilProc.GetSQL(1)), System.Net.Mime.MediaTypeNames.Application.Octet);
        }

        private static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}

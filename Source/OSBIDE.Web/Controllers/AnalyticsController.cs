using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using OSBIDE.Data.SQLDatabase;
using OSBIDE.Web.Models.Analytics;
using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase.DataAnalytics;

namespace OSBIDE.Web.Controllers
{
    //[AllowAccess(SystemRole.Instructor)]
    public class AnalyticsController : ControllerBase
    {
        public ActionResult Criteria()
        {
            var analytics = Analytics.FromSession();
            if (analytics.Criteria == null)
            {
                analytics.Criteria = new Criteria();
            }
            return View("Criteria", analytics.Criteria);
        }

        [HttpPost]
        public ActionResult Refine(Criteria criteria)
        {
            var analytics = Analytics.FromSession();

            analytics.Criteria = criteria;
            analytics.ProcedureData = ProcedureDataProc.Get(criteria)
                                                       .Select(x => new ProcedureDataItem
                                                       {
                                                           IsSelected = true,
                                                           Id = x.Id,
                                                           Name = x.Name,
                                                           Gender = x.Gender.ToString(),
                                                           Age = x.Age,
                                                           Class = x.Class,
                                                           Deliverable = x.Deliverable,
                                                           Quarter = x.Quarter,
                                                           Grade = x.Grade,
                                                           Ethnicity = x.Ethnicity
                                                       })
                                                       .ToList();
            analytics.SelectDataItems = null;

            return View("Refine", analytics.ProcedureData);
        }

        public ActionResult Refine()
        {
            return View("Refine", Analytics.FromSession().ProcedureData);
        }

        [HttpPost]
        public ActionResult Procedure(List<int> selectDataItems)
        {
            var analytics = Analytics.FromSession();
            analytics.SelectDataItems = selectDataItems;

            if (analytics.ProcedureSettings == null)
            {
                analytics.ProcedureSettings = new ProcedureSettings
                                                    {
                                                        SelectedProcedureType = ProcedureType.ErrorQuotient,
                                                        ProcedureParams = new ErrorQuotientParams()
                                                    };
            }
            return View("Procedure", analytics.ProcedureSettings);
        }

        public ActionResult Procedure()
        {
            return View("Procedure", Analytics.FromSession().ProcedureSettings);
        }

        [HttpPost]
        public ActionResult Results(ErrorQuotientParams procedureParams)
        {
            var analytics = Analytics.FromSession();

            analytics.ProcedureSettings.ProcedureParams = procedureParams;
            if (analytics.ProcedureResults == null)
            {
                analytics.ProcedureResults = new ProcedureResults
                {
                    ViewType = ResultViewType.Tabular,
                    Results = ErrorQuotientAnalytics.GetResults(procedureParams, analytics.Criteria.DateFrom, analytics.Criteria.DateTo, analytics.SelectDataItems),
                };

                foreach (var r in analytics.ProcedureResults.Results)
                {
                    var user = analytics.ProcedureData.Where(d => d.Id == r.UserId).First();
                    r.Grade = user.Grade;
                    r.Name = user.Name;
                }
            }

            return View("Results", analytics.ProcedureResults);
        }

        [HttpPost]
        public ActionResult Charts(ProcedureResults procedureResults)
        {
            var analytics = Analytics.FromSession();

            analytics.ProcedureResults.ViewType = procedureResults.ViewType;

            return View("Results", analytics.ProcedureResults);
        }
    }
}
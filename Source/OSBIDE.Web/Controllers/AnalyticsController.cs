using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using OSBIDE.Data.SQLDatabase;
using OSBIDE.Web.Helpers;
using OSBIDE.Web.Models.Analytics;
using OSBIDE.Data.DomainObjects;

namespace OSBIDE.Web.Controllers
{
    //[AllowAccess(SystemRole.Instructor)]
    public class AnalyticsController : ControllerBase
    {
        public ActionResult Index()
        {
            return View(new Analytics
            {
                WizardStep = WizardSteps.Criteria,
                Criteria = new Criteria(),
            });
        }

        [HttpPost]
        [ActionName("Analytics")]
        [ActionSelector(Name = "Criteria")]
        public ActionResult GetProcedureData(Criteria criteria)
        {
            return View("Index", new Analytics
            {
                WizardStep = WizardSteps.Refine,
                ProcedureData = StudentDemographicInfoProc.Get(criteria)
                    .Select(x => new ProcedureDataItem
                    {
                        IsSelected = true,
                        Id = x.Id,
                        Name = x.Name,
                        Gender = x.Gender,
                        Age = x.Age,
                        Class = x.Class,
                        Year = x.Year,
                        Quarter = x.Quarter,
                        Grade = x.Grade,
                        OverallGrade = x.OverallGrade,
                        Ethnicity = x.Ethnicity
                    })
                    .ToList()
            });
        }

        [HttpPost]
        [ActionName("Analytics")]
        [ActionSelector(Name = "Refine")]
        public ActionResult RefineDataSelection(List<int> selectDataItems)
        {
            return View("Index", new Analytics
            {
                WizardStep = WizardSteps.Procedure,
                SelectDataItems = selectDataItems,
            });
        }

        [HttpPost]
        [ActionName("Analytics")]
        [ActionSelector(Name = "Procedure")]
        public ActionResult RunProcedure(ErrorQuotientParam procedureParams)
        {
            return View("Index", new Analytics
            {
                WizardStep = WizardSteps.Results,
                ProcedureParams = procedureParams,
            });
        }
    }
}
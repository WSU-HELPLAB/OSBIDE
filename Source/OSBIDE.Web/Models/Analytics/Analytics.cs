using System.Collections.Generic;

using OSBIDE.Data.DomainObjects;

namespace OSBIDE.Web.Models.Analytics
{
    public class Analytics
    {
        public WizardSteps WizardStep { get; set; }
        public Criteria Criteria { get; set; }
        public List<ProcedureDataItem> ProcedureData { get; set; }
        public List<int> SelectDataItems { get; set; }
        public List<ProcedureType> ProcedureTypes { get; set; }
        public ProcedureType SelectedProcedureType { get; set; }
        public dynamic ProcedureParams { get; set; }
        public dynamic ProcedureResult { get; set; }
    }
}
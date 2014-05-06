using OSBIDE.Data.DomainObjects;

namespace OSBIDE.Web.Models.Analytics
{
    public class ProcedureResults
    {
        public ResultViewType ViewType { get; set; }
        public dynamic Results { get; set; }
    }
}
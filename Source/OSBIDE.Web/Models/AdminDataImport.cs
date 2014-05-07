using System.Web;

namespace OSBIDE.Web.Models
{
    public class AdminDataImport
    {
        public HttpPostedFileBase File { get; set; }
        public int CourseId { get; set; }
        public string Deliverable { get; set; }
    }
}
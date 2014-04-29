using System.Web;

namespace OSBIDE.Web.Models
{
    public class AdminDataImport
    {
        public HttpPostedFileBase File { get; set; }
        public int Year { get; set; }
        public string Semester { get; set; }
    }
}
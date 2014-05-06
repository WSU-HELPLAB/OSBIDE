using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Data.DomainObjects
{
    /// <summary>
    /// The collated compilation event for error quotient procedure
    /// </summary>
    public class ErrorQuotientEvent
    {
        public int BuildId { get; set; }
        public int LogId { get; set; }
        public int UserId { get; set; }
        public DateTime EventDate { get; set; }
        public List<ErrorDocumentInfo> Documents { get; set; }
        public List<int> ErrorTypeIds { get; set; }
    }

    public class ErrorDocumentInfo
    {
        public int DocumentId { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
    }
}

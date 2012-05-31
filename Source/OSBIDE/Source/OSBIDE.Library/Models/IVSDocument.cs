using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    public interface IVSDocument
    {
        string FileName { get; set; }
        string Content { get; set; }
        List<BreakPoint> BreakPoints { get; set; }
        List<ErrorListItem> ErrorItems { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class CodeDocument : IVSDocument
    {
        public string FileName { get; set; }
        public string Content { get; set; }
        public List<BreakPoint> BreakPoints { get; set; }
        public List<ErrorListItem> ErrorItems { get; set; }

        public CodeDocument()
        {
            BreakPoints = new List<BreakPoint>();
            ErrorItems = new List<ErrorListItem>();
        }
    }
}

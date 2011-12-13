using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE80;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class ErrorListItem
    {
        public int Column { get; set; }
        public int Line { get; set; }
        public string File { get; set; }
        public string Project { get; set; }
        public string Description { get; set; }
        
        public static ErrorListItem FromErrorItem(ErrorItem item)
        {
            ErrorListItem eli = new ErrorListItem();
            eli.Column = item.Column;
            eli.Line = item.Line;
            eli.File = item.FileName;
            eli.Description = item.Description;

            //Sometimes project references are invalid.  Not sure why.
            eli.Project = "Unknown";
            try
            {
                eli.Project = item.Project;
            }
            catch (Exception ex)
            {
                
            }

            return eli;
        }
    }
}

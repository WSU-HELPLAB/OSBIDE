using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE80;
using System.ComponentModel.DataAnnotations;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class ErrorListItem
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int Column { get; set; }

        [Required]
        public int Line { get; set; }

        [Required]
        public string File { get; set; }

        [Required]
        public string Project { get; set; }

        [Required]
        public string Description { get; set; }
        
        public static ErrorListItem FromErrorItem(ErrorItem item)
        {
            ErrorListItem eli = new ErrorListItem();

            //Sometimes ErrorItem references are invalid.  Not sure why.
            try
            {
                eli.Project = item.Project;
                eli.Column = item.Column;
                eli.Line = item.Line;
                eli.File = item.FileName;
                eli.Description = item.Description;
            }
            catch (Exception)
            {
                
            }

            return eli;
        }
    }
}

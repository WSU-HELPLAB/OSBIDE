using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using OSBIDE.Library.Events;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class CodeDocument : IVSDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public string Content { get; set; }

        public List<CodeDocumentBreakPoint> BreakPoints { get; set; }
        public List<CodeDocumentErrorListItem> ErrorItems { get; set; }

        public CodeDocument()
        {
            BreakPoints = new List<CodeDocumentBreakPoint>();
            ErrorItems = new List<CodeDocumentErrorListItem>();
        }
    }
}

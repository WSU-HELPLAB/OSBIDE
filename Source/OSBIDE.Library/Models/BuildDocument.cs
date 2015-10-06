using OSBIDE.Library.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class BuildDocument
    {
        [Key]
        [Column(Order=0)]
        public int BuildId { get; set; }
        
        [ForeignKey("BuildId")]
        public virtual BuildEvent Build { get; set; }

        [Key]
        [Column(Order = 1)]
        public int DocumentId { get; set; }

        [ForeignKey("DocumentId")]
        public virtual CodeDocument Document { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using OSBIDE.Library.Events;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class BuildEventErrorListItem
    {
        [Key]
        [Required]
        [Column(Order = 0)]
        public int BuildEventId { get; set; }

        [ForeignKey("BuildEventId")]
        public virtual BuildEvent BuildEvent { get; set; }

        [Key]
        [Required]
        [Column(Order = 1)]
        public int ErrorListItemId { get; set; }

        [ForeignKey("ErrorListItemId")]
        public virtual ErrorListItem ErrorListItem { get; set; }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using OSBIDE.Library.Events;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class BuildEventBreakPoint
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
        public int BreakPointId { get; set; }

        [ForeignKey("BreakPointId")]
        public virtual BreakPoint BreakPoint { get; set; }
    }
}
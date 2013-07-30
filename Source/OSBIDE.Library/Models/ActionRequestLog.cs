﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    public class ActionRequestLog
    {
        public const string ACTION_PARAMETER_DELIMITER = "|||";
        public const string ACTION_PARAMETER_NULL_VALUE = "[null]";

        [Key]
        public int Id { get; set; }

        public int CreatorId { get; set; }

        [ForeignKey("CreatorId")]
        public OsbideUser Creator { get; set; }

        public string ActionName { get; set; }

        public string ActionParameters { get; set; }

        public string ControllerName { get; set; }

        public DateTime AccessDate { get; set; }

        public ActionRequestLog()
        {
            AccessDate = DateTime.UtcNow;
        }
    }
}
using System;
using System.ComponentModel.DataAnnotations;

using OSBIDE.Library.Models;

namespace OSBIDE.Data.DomainObjects
{
    public class Criteria
    {
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? DateFrom { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? DateTo { get; set; }
        public string NameToken { get; set; }
        public int? StudentId { get; set; }
        public int GenderId { get; set; }
        public int AgeFrom { get; set; }
        public int AgeTo { get; set; }
        public int CourseId { get; set; }
        public string Deliverable { get; set; }
        public decimal? GradeFrom { get; set; }
        public decimal? GradeTo { get; set; }
    }
}

using System;
using OSBIDE.Library.Models;

namespace OSBIDE.Data.DomainObjects
{
    public class Criteria
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string NameToken { get; set; }
        public string Gender { get; set; }
        public int GenderId { get; set; }
        public int AgeFrom { get; set; }
        public int AgeTo { get; set; }
        public string Course { get; set; }
        public int CourseId { get; set; }
        public int Year { get; set; }
        public decimal GradeFrom { get; set; }
        public decimal GradeTo { get; set; }
        public decimal OverallGradeFrom { get; set; }
        public decimal OverallGradeTo { get; set; }
    }
}
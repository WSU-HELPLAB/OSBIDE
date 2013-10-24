using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.Library.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSBIDE.Library.Models
{
    public class Course : IModelBuilderExtender
    {
        [Key]
        [Required]
        [Column(Order = 0)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("School")]
        public int SchoolId { get; set; }
        public virtual School School { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public virtual IList<CourseCoordinator> Coordinators { get; set; }

        [Required]
        public virtual IList<CourseStudent> Students { get; set; }

        [Required]
        public virtual IList<CourseAssistant> Assistants { get; set; }

        public Course()
        {
            Description = "";
            Coordinators = new List<CourseCoordinator>();
            Students = new List<CourseStudent>();
            Assistants = new List<CourseAssistant>();
        }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            
        }
    }
}

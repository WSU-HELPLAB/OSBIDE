using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.Library.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class Course : IModelBuilderExtender, IComparer<Course>, IComparable<Course>
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

        [Required(AllowEmptyStrings=true)]
        public string Description { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public string Season { get; set; }

        [Required]
        public bool RequiresApprovalBeforeAdmission { get; set; }

        [Required]
        public bool IsDeleted { get; set; }

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
            RequiresApprovalBeforeAdmission = false;
            IsDeleted = false;
        }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            
        }

        public int Compare(Course x, Course y)
        {
            return x.Id.CompareTo(y.Id);
        }

        public int CompareTo(Course other)
        {
            return this.Id.CompareTo(other.Id);
        }
    }
}

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

        [NotMapped]
        public string Name
        {
            get
            {
                return string.Format("{0} {1}", Prefix, CourseNumber);
            }
            set
            {
                string[] pieces = value.Split(' ');
                if (pieces.Length != 2)
                {
                    throw new Exception("Unexpected input.  Expected name in the format \"<prefix> <course number>\".");
                }
                Prefix = pieces[0];
                CourseNumber = pieces[1];
            }
        }

        [Required]
        public string Prefix { get; set; }

        [Required]
        public string CourseNumber { get; set; }

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

        public virtual IList<CourseUserRelationship> CourseUserRelationships { get; set; }

        public Course()
        {
            Description = "";
            CourseUserRelationships = new List<CourseUserRelationship>();
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

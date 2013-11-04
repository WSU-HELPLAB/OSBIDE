using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Library.Models
{
    /// <summary>
    /// Represents a link between a course and a given user.  This type of link is most
    /// common for students in a course.
    /// </summary>
    [Serializable]
    public class CourseStudent : IModelBuilderExtender, ICourseUserRelationship
    {
        [Key]
        [Column(Order = 2)]
        [Required]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        [Key]
        [Column(Order = 1)]
        [Required]
        public int UserId { get; set; }
        public virtual OsbideUser User { get; set; }

        public bool IsActive { get; set; }

        public CourseRelationship Relationship
        {
            get
            {
                return CourseRelationship.Student;
            }
        }

        public CourseStudent()
        {
            IsActive = true;
        }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CourseStudent>()
                .HasRequired(c => c.Course)
                .WithMany(c => c.Students)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CourseStudent>()
                .HasRequired(c => c.User)
                .WithMany()
                .WillCascadeOnDelete(false);
        }

        public int CompareTo(ICourseUserRelationship other)
        {
            if (Relationship == other.Relationship)
            {
                return Course.Name.CompareTo(other.Course.Name);
            }
            else
            {
                return Relationship.CompareTo(other.Relationship);
            }
        }

        public int Compare(ICourseUserRelationship x, ICourseUserRelationship y)
        {
            if (x.Relationship == y.Relationship)
            {
                return x.Course.Name.CompareTo(y.Course.Name);
            }
            else
            {
                return x.Relationship.CompareTo(y.Relationship);
            }
        }
    }
}

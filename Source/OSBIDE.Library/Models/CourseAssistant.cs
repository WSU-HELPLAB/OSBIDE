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
    /// Course assistants are people who help in running the course, but do not have complete admin access to the
    /// given course.  TAs commonly fit this role.
    /// </summary>
    [Serializable]
    public class CourseAssistant : IModelBuilderExtender, ICourseUserRelationship
    {
        [Key]
        [Column(Order = 0)]
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
                return CourseRelationship.Assistant;
            }
        }

        public CourseAssistant()
        {
            IsActive = true;
        }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CourseAssistant>()
                .HasRequired(c => c.Course)
                .WithMany(c => c.Assistants)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CourseAssistant>()
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

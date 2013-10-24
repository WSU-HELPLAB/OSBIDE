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
    public class CourseStudent : IModelBuilderExtender
    {
        [Key]
        [Column(Order = 2)]
        [Required]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        [Key]
        [Column(Order = 1)]
        [Required]
        public int StudentId { get; set; }
        public virtual OsbideUser Student { get; set; }

        public bool IsActive { get; set; }

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
                .HasRequired(c => c.Student)
                .WithMany()
                .WillCascadeOnDelete(false);
        }
    }
}

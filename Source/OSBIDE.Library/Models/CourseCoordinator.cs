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
    /// Course coordinators oversee a given class within OSBIDE.  Instructors would be
    /// considered coordinators.
    /// </summary>
    public class CourseCoordinator : IModelBuilderExtender
    {
        [Key]
        [Column(Order = 0)]
        [Required]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        [Key]
        [Column(Order = 1)]
        [Required]
        public int CoordinatorId { get; set; }
        public virtual OsbideUser Coordinator { get; set; }

        public bool IsActive { get; set; }

        public CourseCoordinator()
        {
            IsActive = true;
        }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CourseCoordinator>()
                .HasRequired(c => c.Course)
                .WithMany(c => c.Coordinators)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CourseCoordinator>()
                .HasRequired(c => c.Coordinator)
                .WithMany()
                .WillCascadeOnDelete(false);
        }
    }
}

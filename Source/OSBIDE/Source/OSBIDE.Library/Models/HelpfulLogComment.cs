using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    public class HelpfulLogComment : IModelBuilderExtender
    {
        [Key]
        [Column(Order=0)]
        public int CommentId { get; set; }
        public virtual LogComment Comment { get; set; }

        [Key]
        [Column(Order = 1)]
        public int UserId { get; set; }
        public virtual OsbideUser User { get; set; }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HelpfulLogComment>()
                .HasRequired(m => m.User)
                .WithMany()
                .WillCascadeOnDelete(false);
        }
    }
}

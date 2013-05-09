using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    //TODO: Write custom delete trigger for this model.
    public class LogComment : IModelBuilderExtender
    {
        [Key]
        public int Id { get; set; }

        public int LogId { get; set; }

        [ForeignKey("LogId")]
        public virtual EventLog Log { get; set; }

        public int AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public virtual OsbideUser Author { get; set; }

        public DateTime DatePosted { get; set; }

        public string Content { get; set; }

        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public LogComment Parent { get; set; }

        public virtual IList<LogComment> ChildComments { get; set; }

        public virtual IList<HelpfulLogComment> HelpfulMarks { get; set; }

        public LogComment()
        {
            DatePosted = DateTime.Now;
            ChildComments = new List<LogComment>();
            HelpfulMarks = new List<HelpfulLogComment>();
        }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LogComment>()
                .HasRequired(c => c.Author)
                .WithMany()
                .WillCascadeOnDelete(false);
        }
    }
}

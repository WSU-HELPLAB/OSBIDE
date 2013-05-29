using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
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

        private int _helpfulMarkCount = 0;

        /// <summary>
        /// Returns the number of helpful marks recorded since the last call to <see cref="UpdateHelpfulMarkCount"/>.  This propery, while programmatically unnecessary,
        /// allows proper serialization of the total number of helpful marks received by the current
        /// comment.
        /// </summary>
        public int HelpfulMarkCount
        {
            get
            {
                return _helpfulMarkCount;
            }
        }

        [NonSerialized]
        [IgnoreDataMember]
        private IList<HelpfulLogComment> _helpfulMarks;
        public virtual IList<HelpfulLogComment> HelpfulMarks
        {
            get
            {
                return _helpfulMarks;
            }
            set
            {
                _helpfulMarks = value;
            }
        }

        public LogComment()
        {
            DatePosted = DateTime.Now;
            ChildComments = new List<LogComment>();
            HelpfulMarks = new List<HelpfulLogComment>();
        }

        public LogComment(LogComment other)
            : this()
        {
            if (other != null)
            {
                Id = other.Id;
                LogId = other.LogId;
                AuthorId = other.AuthorId;
                Author = new OsbideUser(other.Author);
                DatePosted = other.DatePosted;
                Content = other.Content;
                ParentId = other.ParentId;
                Parent = new LogComment(other.Parent);
                try
                {
                    foreach (LogComment child in other.ChildComments)
                    {
                        ChildComments.Add(new LogComment(child));
                    }
                }
                catch (Exception)
                {
                }
                //AC: adding helpful marks may cause a stack overflow.
                /*
                foreach (HelpfulLogComment helpful in other.HelpfulMarks)
                {
                    HelpfulMarks.Add(new HelpfulLogComment(helpful));
                }*/
            }
        }

        /// <summary>
        /// See <see cref="UpdateHelpfulMarkCount"/> for a justification of this function.
        /// </summary>
        public void UpdateHelpfulMarkCount()
        {
            if (HelpfulMarks != null)
            {
                _helpfulMarkCount = HelpfulMarks.Count;
            }
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

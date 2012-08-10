using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using EnvDTE;
using OSBIDE.Library.Events;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class StackFrame : IModelBuilderExtender
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FunctionName { get; set; }

        [Required]
        public string Module { get; set; }

        [Required]
        public string Language { get; set; }

        [Required]
        public int ExceptionEventId { get; set; }

        [Required]
        public string ReturnType { get; set; }

        [ForeignKey("ExceptionEventId")]
        public virtual ExceptionEvent Exception { get; set; }

        /// <summary>
        /// The depth of the stack frame.  A depth of 0 means that it is the top
        /// most stack frame.
        /// </summary>
        [Required]
        public int Depth { get; set; }

        public virtual IList<StackFrameVariable> Variables { get; set; }

        public StackFrame()
        {
            Variables = new List<StackFrameVariable>();
        }

        public StackFrame(EnvDTE.StackFrame frame)
            : this()
        {
            this.FunctionName = frame.FunctionName;
            this.Module = frame.Module;
            this.Language = frame.Language;
            this.ReturnType = frame.ReturnType;
            foreach (Expression local in frame.Locals)
            {
                StackFrameVariable var = new StackFrameVariable()
                {
                    Name = local.Name,
                    Value = local.Value
                };
            }
        }

        public void BuildRelationship(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StackFrame>()
                .HasMany(sf => sf.Variables)
                .WithRequired(v => v.StackFrame)
                .WillCascadeOnDelete(true);
        }
    }
}

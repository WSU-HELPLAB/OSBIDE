using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class EventLogSubscription : IModelBuilderExtender
    {
        [Key]
        [Column(Order = 0)]
        [Required]
        public int UserId { get; set; }

        [Key]
        [Column(Order = 1)]
        [Required]
        public int LogId { get; set; }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
        }
    }
}

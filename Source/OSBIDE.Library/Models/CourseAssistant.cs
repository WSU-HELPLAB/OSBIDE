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
    public class CourseAssistant
    {
        [Key]
        [Column(Order = 0)]
        [Required]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        [Key]
        [Column(Order = 1)]
        [Required]
        public int AssistantId { get; set; }
        public virtual OsbideUser Assistant { get; set; }

        public bool IsActive { get; set; }

        public CourseAssistant()
        {
            IsActive = true;
        }
    }
}

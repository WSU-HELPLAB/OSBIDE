using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.Library.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSBIDE.Library.Models
{
    public class Course
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public virtual IList<CourseCoordinator> Coordinators { get; set; }

        [Required]
        public virtual IList<CourseStudent> Students { get; set; }

        [Required]
        public virtual IList<CourseAssistant> Assistants { get; set; }

        public Course()
        {
            Description = "";
            Coordinators = new List<CourseCoordinator>();
            Students = new List<CourseStudent>();
            Assistants = new List<CourseAssistant>();
        }
    }
}

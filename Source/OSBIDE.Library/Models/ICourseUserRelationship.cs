using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Library.Models
{
    public interface ICourseUserRelationship
    {
        int CourseId { get; set; }
        Course Course { get; set; }

        int UserId { get; set; }
        OsbideUser User { get; set; }

        bool IsActive { get; set; }
    }
}

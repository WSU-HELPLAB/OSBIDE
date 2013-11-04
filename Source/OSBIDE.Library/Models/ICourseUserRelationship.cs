using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Library.Models
{
    public enum CourseRelationship { Student, Assistant, Coordinator }

    public interface ICourseUserRelationship : IComparable<ICourseUserRelationship>, IComparer<ICourseUserRelationship>
    {
        int CourseId { get; set; }
        Course Course { get; set; }

        int UserId { get; set; }
        OsbideUser User { get; set; }

        bool IsActive { get; set; }

        CourseRelationship Relationship { get; }

        int CompareTo(ICourseUserRelationship other);

        int Compare(ICourseUserRelationship x, ICourseUserRelationship y);
    }
}

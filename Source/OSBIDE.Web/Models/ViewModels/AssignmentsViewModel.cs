using OSBIDE.Library.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models.ViewModels
{
    public class AssignmentsViewModel
    {
        public List<string> AssignmentNames { get; set; }
        public string CurrentAssignmentName { get; set; }
        public List<SubmitEvent> Assignments { get; set; }
    }
}
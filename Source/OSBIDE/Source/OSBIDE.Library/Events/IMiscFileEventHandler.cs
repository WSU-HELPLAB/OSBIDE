using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;

namespace OSBIDE.Library.Events
{
    public interface IMiscFileEventHandler
    {
        void ProjectItemAdded(ProjectItem ProjectItem);
        void ProjectItemRemoved(ProjectItem ProjectItem);
        void ProjectItemRenamed(ProjectItem ProjectItem, string OldName);
    }
}

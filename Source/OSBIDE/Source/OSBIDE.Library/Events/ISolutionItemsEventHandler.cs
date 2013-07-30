using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;

namespace OSBIDE.Library.Events
{
    public interface ISolutionItemsEventHandler
    {
        void SolutionItemAdded(ProjectItem ProjectItem);
        void SolutionItemRemoved(ProjectItem ProjectItem);
        void SolutionItemRenamed(ProjectItem ProjectItem, string OldName);
    }
}

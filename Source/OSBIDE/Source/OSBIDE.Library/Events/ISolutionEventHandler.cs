using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;

namespace OSBIDE.Library.Events
{
    public interface ISolutionEventHandler
    {
        void SolutionBeforeClosing();
        void SolutionOpened();
        void ProjectAdded(Project Project);
        void SolutionRenamed(string OldName);
    }
}

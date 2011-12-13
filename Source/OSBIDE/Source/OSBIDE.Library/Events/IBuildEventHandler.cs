using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;

namespace OSBIDE.Library.Events
{
    public interface IBuildEventHandler
    {
        void OnBuildBegin(vsBuildScope Scope, vsBuildAction Action);
        void OnBuildDone(vsBuildScope Scope, vsBuildAction Action);
    }
}

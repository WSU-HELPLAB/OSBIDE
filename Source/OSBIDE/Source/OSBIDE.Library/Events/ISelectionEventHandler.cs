using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;

namespace OSBIDE.Library.Events
{
    public interface ISelectionEventHandler
    {
        void SelectionChange();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;

namespace OSBIDE.Library.Events
{
    public interface IDocumentEventHandler
    {
        void DocumentClosing(Document Document);
        void DocumentOpened(Document Document);
        void DocumentSaved(Document Document);
    }
}

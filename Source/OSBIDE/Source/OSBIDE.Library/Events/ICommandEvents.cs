using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Events
{
    public interface ICommandEvents
    {
        void AfterCommandExecute(string Guid, int ID, object CustomIn, object CustomOut);
        void BeforeCommandExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault);
    }
}

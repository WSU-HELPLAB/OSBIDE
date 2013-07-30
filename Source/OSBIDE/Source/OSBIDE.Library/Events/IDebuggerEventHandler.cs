using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;

namespace OSBIDE.Library.Events
{
    public interface IDebuggerEventHandler
    {
        void OnEnterBreakMode(dbgEventReason Reason, ref dbgExecutionAction ExecutionAction);
        void OnEnterDesignMode(dbgEventReason Reason);
        void OnEnterRunMode(dbgEventReason Reason);
        void OnExceptionNotHandled(string ExceptionType, string Name, int Code, string Description, ref dbgExceptionAction ExceptionAction);
        void OnExceptionThrown(string ExceptionType, string Name, int Code, string Description, ref dbgExceptionAction ExceptionAction);
    }
}

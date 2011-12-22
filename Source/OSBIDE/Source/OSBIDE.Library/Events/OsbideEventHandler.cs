using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;
using OSBIDE.Library.Models;

namespace OSBIDE.Library.Events
{
    public class OsbideEventHandler : EventHandlerBase
    {
        public OsbideEventHandler(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {

        }

        public override void DocumentSaved(Document Document)
        {
            //create the osbide event
            TextSelection selection = Document.Selection;
            
            selection.StartOfDocument();
            selection.EndOfDocument(true);
            string document = selection.Text;
            SaveEvent save = new SaveEvent();
            save.EventDate = DateTime.Now;
            save.SolutionName = dte.Solution.FullName;
            save.DocumentName = Document.FullName;
            save.DocumentContent = document;

            //TODO: Reset cursor position
            
            //let others know that we have a new event
            NotifyEventCreated(this, new EventCreatedArgs(save));
        }

        public override void OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
        {
            BuildEvent build = new BuildEvent();
            build.SolutionName = dte.Solution.FullName;
            build.EventDate = DateTime.Now;

            //start at 1 when iterating through Error List
            for (int i = 1; i <= dte.ToolWindows.ErrorList.ErrorItems.Count; i++)
            {
                ErrorItem item = dte.ToolWindows.ErrorList.ErrorItems.Item(i);
                build.ErrorItems.Add(ErrorListItem.FromErrorItem(item));
            }
            byte[] data = EventFactory.ToZippedBinary(build);

            //let others know that we have created a new event
            NotifyEventCreated(this, new EventCreatedArgs(build));
        }

        public override void OnEnterBreakMode(dbgEventReason Reason, ref dbgExecutionAction ExecutionAction)
        {
            DebugEvent debug = new DebugEvent();
            debug.SolutionName = dte.Solution.FullName;
            debug.EventDate = DateTime.Now;
            debug.EventReason = (int)Reason;
            debug.ExecutionAction = (int)ExecutionAction;
            debug.DocumentName = dte.ActiveDocument.FullName;

            //let others know that we have created a new event
            NotifyEventCreated(this, new EventCreatedArgs(debug));
        }

        public override void OnEnterDesignMode(dbgEventReason Reason)
        {
            dbgExecutionAction action = dbgExecutionAction.dbgExecutionActionDefault;
            OnEnterBreakMode(Reason, ref action);
        }

        public override void OnEnterRunMode(dbgEventReason Reason)
        {
            dbgExecutionAction action = dbgExecutionAction.dbgExecutionActionDefault;
            OnEnterBreakMode(Reason, ref action);
        }

        public override void OnExceptionNotHandled(string ExceptionType, string Name, int Code, string Description, ref dbgExceptionAction ExceptionAction)
        {
            OnExceptionThrown(ExceptionType, Name, Code, Description, ref ExceptionAction);
        }

        public override void OnExceptionThrown(string ExceptionType, string Name, int Code, string Description, ref dbgExceptionAction ExceptionAction)
        {
            ExceptionEvent ext = new ExceptionEvent()
            {
                EventDate = DateTime.Now,
                ExceptionAction = (int)ExceptionAction,
                ExceptionCode = Code,
                ExceptionDescription = Description,
                ExceptionName = Name,
                ExceptionType = ExceptionType,
                SolutionName = dte.Solution.FullName,
                DocumentName = dte.ActiveDocument.FullName
            };

            //let others know that we have created a new event
            NotifyEventCreated(this, new EventCreatedArgs(ext));
        }

        //TODO: Handle ling change events
        public override void EditorLineChanged(TextPoint StartPoint, TextPoint EndPoint, int Hint)
        {
            
        }
    }
}

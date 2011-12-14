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
            
        }

        public override void OnEnterDesignMode(dbgEventReason Reason)
        {
            
        }

        public override void OnEnterRunMode(dbgEventReason Reason)
        {
            
        }

        public override void OnExceptionNotHandled(string ExceptionType, string Name, int Code, string Description, ref dbgExceptionAction ExceptionAction)
        {
            
        }

        public override void OnExceptionThrown(string ExceptionType, string Name, int Code, string Description, ref dbgExceptionAction ExceptionAction)
        {
            
        }

        public override void EditorLineChanged(TextPoint StartPoint, TextPoint EndPoint, int Hint)
        {
            
        }
    }
}

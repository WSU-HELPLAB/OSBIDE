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
            int currentLine = selection.ActivePoint.Line;
            int offset = selection.ActivePoint.DisplayColumn;

            selection.StartOfDocument();
            selection.EndOfDocument(true);
            string document = selection.Text;

            //Reset cursor position
            selection.MoveToDisplayColumn(currentLine, offset);

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

        public override void BeforeCommandExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
        {
            Command cmd = dte.Commands.Item(Guid, ID);
            string commandName = "";
            if (cmd != null)
            {
                commandName = cmd.Name;

                //These events constantly fire and are of no use to us.  As such, speed up the process by always ignoring
                //them
                if (commandName.CompareTo("Build.SolutionConfigurations") != 0 && commandName.CompareTo("Edit.GoToFindCombo") != 0)
                {
                    IOsbideEvent oEvent = EventFactory.FromCommand(commandName, dte);

                    //protect against the off-chance that we'll get a null return value
                    if (oEvent != null)
                    {
                        //let others know that we have created a new event
                        NotifyEventCreated(this, new EventCreatedArgs(oEvent));
                    }
                }
            }
        }

        //TODO: Handle ling change events
        public override void EditorLineChanged(TextPoint StartPoint, TextPoint EndPoint, int Hint)
        {
            
        }
    }
}

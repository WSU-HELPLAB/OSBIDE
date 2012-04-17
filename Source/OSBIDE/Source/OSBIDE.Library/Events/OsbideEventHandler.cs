﻿using System;
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
        /// <summary>
        /// These events constantly fire and are of no use to us.
        /// </summary>
        private List<string> boringCommands =
            (new string[] 
                {
                    "Build.SolutionConfigurations",
                    "Edit.GoToFindCombo",
                    ""
                }
            ).ToList();
        private DateTime LastEditorActivityEvent = DateTime.MinValue;
        public enum BreakpointIDs
        {
            BreakAtFunction = 311,
            EditorClick = 769
        };

        public OsbideEventHandler(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {

        }

        private Command GetCommand(string guid, int id)
        {
            Command cmd = null;
            try
            {
                cmd = dte.Commands.Item(guid, id);
            }
            catch (Exception ex)
            {
                //do nothing
            }
            return cmd;
        }

        #region EventHandlerBase Overrides

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

            //add in breakpoint information
            for (int i = 1; i < dte.Debugger.Breakpoints.Count; i++)
            {
                BreakPoint bp = new BreakPoint(dte.Debugger.Breakpoints.Item(i));
                build.Breakpoints.Add(bp);
            }

            byte[] data = EventFactory.ToZippedBinary(build);

            //let others know that we have created a new event
            NotifyEventCreated(this, new EventCreatedArgs(build));
        }

        public override void MenuCommand_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
        {
            Command cmd = GetCommand(Guid, ID);
            List<int> breakpointIds = Enum.GetValues(typeof(BreakpointIDs)).Cast<int>().ToList();
            if (breakpointIds.Contains(cmd.ID))
            {

            }
        }

        public override void GenericCommand_BeforeCommandExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
        {
            Command cmd = GetCommand(Guid, ID);
            string commandName = "";
            if (cmd != null)
            {
                commandName = cmd.Name;

                //Speed up the process by always ignoring boring commands
                if (boringCommands.Contains(commandName) == false)
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

        /// <summary>
        /// Called whenever the current line gets modified (text added / deleted).  Only raised at a maximum of
        /// once per minute in order to undercut the potential flood of event notifications.
        /// </summary>
        /// <param name="StartPoint"></param>
        /// <param name="EndPoint"></param>
        /// <param name="Hint"></param>
        public override void EditorLineChanged(TextPoint StartPoint, TextPoint EndPoint, int Hint)
        {
            if (LastEditorActivityEvent < DateTime.Now.Subtract(new TimeSpan(0, 1, 0)))
            {
                LastEditorActivityEvent = DateTime.Now;
                EditorActivityEvent activity = new EditorActivityEvent();
                activity.EventDate = DateTime.Now;
                activity.SolutionName = dte.Solution.FullName;
                NotifyEventCreated(this, new EventCreatedArgs(activity));
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;

namespace OSBIDE.Library.Events
{
    /// <summary>
    /// The EventHandlerBase class consolidates all of the various event handler types into a single class for
    /// easy inheritance.  By default, each event handler does nothing.  
    /// </summary>
    public abstract class EventHandlerBase : 
        IBuildEventHandler, 
        IDebuggerEventHandler, 
        IDocumentEventHandler, 
        IFindEventHandler, 
        IMiscFileEventHandler, 
        IOutputWindowEventHandler, 
        ISelectionEventHandler, 
        ISolutionEventHandler, 
        ISolutionItemsEventHandler, 
        ITextEditorEventHandler
    {
        /// <summary>
        /// This event is raised whenever a new event log has been created and is ready for consumption
        /// </summary>
        public event EventHandler<EventCreatedArgs> EventCreated = delegate { };

        protected DTE2 dte {
            get
            {
                DTE2 dteRef = null;
                if (ServiceProvider != null)
                {
                    dteRef = (DTE2)ServiceProvider.GetService(typeof(SDTE));
                }
                return dteRef;
            }
        }
        public IServiceProvider ServiceProvider { get; set; }
        private BuildEvents buildEvents = null;
        private DebuggerEvents debuggerEvents = null;
        private DocumentEvents documentEvents = null;
        private FindEvents findEvents = null;
        private ProjectItemsEvents miscFileEvents = null;
        private OutputWindowEvents outputWindowEvents = null;
        private SelectionEvents selectionEvents = null;
        private SolutionEvents solutionEvents = null;
        private ProjectItemsEvents solutionItemsEvents = null;
        private TextEditorEvents textEditorEvents = null;

        public EventHandlerBase(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;

            //save references to dte events
            buildEvents = dte.Events.BuildEvents;
            debuggerEvents = dte.Events.DebuggerEvents;
            documentEvents = dte.Events.DocumentEvents;
            findEvents = dte.Events.FindEvents;
            miscFileEvents = dte.Events.MiscFilesEvents;
            outputWindowEvents = dte.Events.OutputWindowEvents;
            selectionEvents = dte.Events.SelectionEvents;
            solutionEvents = dte.Events.SolutionEvents;
            solutionItemsEvents = dte.Events.SolutionItemsEvents;
            textEditorEvents = dte.Events.TextEditorEvents;

            //attach listeners for dte events
            //build events
            buildEvents.OnBuildBegin += new _dispBuildEvents_OnBuildBeginEventHandler(OnBuildBegin);
            buildEvents.OnBuildDone += new _dispBuildEvents_OnBuildDoneEventHandler(OnBuildDone);

            //debugger events
            debuggerEvents.OnEnterBreakMode += new _dispDebuggerEvents_OnEnterBreakModeEventHandler(OnEnterBreakMode);
            debuggerEvents.OnEnterDesignMode += new _dispDebuggerEvents_OnEnterDesignModeEventHandler(OnEnterDesignMode);
            debuggerEvents.OnEnterRunMode += new _dispDebuggerEvents_OnEnterRunModeEventHandler(OnEnterRunMode);
            debuggerEvents.OnExceptionNotHandled += new _dispDebuggerEvents_OnExceptionNotHandledEventHandler(OnExceptionNotHandled);
            debuggerEvents.OnExceptionThrown += new _dispDebuggerEvents_OnExceptionThrownEventHandler(OnExceptionThrown);

            //document events
            documentEvents.DocumentClosing += new _dispDocumentEvents_DocumentClosingEventHandler(DocumentClosing);
            documentEvents.DocumentOpened += new _dispDocumentEvents_DocumentOpenedEventHandler(DocumentOpened);
            documentEvents.DocumentSaved += new _dispDocumentEvents_DocumentSavedEventHandler(DocumentSaved);

            //find events
            findEvents.FindDone += new _dispFindEvents_FindDoneEventHandler(FindDone);

            //misc file events
            miscFileEvents.ItemAdded += new _dispProjectItemsEvents_ItemAddedEventHandler(ProjectItemAdded);
            miscFileEvents.ItemRemoved += new _dispProjectItemsEvents_ItemRemovedEventHandler(ProjectItemRemoved);
            miscFileEvents.ItemRenamed += new _dispProjectItemsEvents_ItemRenamedEventHandler(ProjectItemRenamed);

            //output window events
            outputWindowEvents.PaneUpdated += new _dispOutputWindowEvents_PaneUpdatedEventHandler(OutputPaneUpdated);

            //selection events
            selectionEvents.OnChange += new _dispSelectionEvents_OnChangeEventHandler(SelectionChange);

            //solution events
            solutionEvents.BeforeClosing += new _dispSolutionEvents_BeforeClosingEventHandler(SolutionBeforeClosing);
            solutionEvents.Opened += new _dispSolutionEvents_OpenedEventHandler(SolutionOpened);
            solutionEvents.ProjectAdded += new _dispSolutionEvents_ProjectAddedEventHandler(ProjectAdded);
            solutionEvents.Renamed += new _dispSolutionEvents_RenamedEventHandler(SolutionRenamed);

            //solution item events
            solutionItemsEvents.ItemAdded += new _dispProjectItemsEvents_ItemAddedEventHandler(SolutionItemAdded);
            solutionItemsEvents.ItemRemoved += new _dispProjectItemsEvents_ItemRemovedEventHandler(SolutionItemRemoved);
            solutionItemsEvents.ItemRenamed += new _dispProjectItemsEvents_ItemRenamedEventHandler(SolutionItemRenamed);

            //text editor events
            textEditorEvents.LineChanged += new _dispTextEditorEvents_LineChangedEventHandler(EditorLineChanged);
        }

        protected void NotifyEventCreated(object sender, EventCreatedArgs eventArgs)
        {
            EventCreated(sender, eventArgs);
        }

        //build event handlers
        public virtual void OnBuildBegin(vsBuildScope Scope, vsBuildAction Action) { }
        public virtual void OnBuildDone(vsBuildScope Scope, vsBuildAction Action) { }

        //debugger event handlers
        public virtual void OnEnterBreakMode(dbgEventReason Reason, ref dbgExecutionAction ExecutionAction) { }
        public virtual void OnEnterDesignMode(dbgEventReason Reason) { }
        public virtual void OnEnterRunMode(dbgEventReason Reason) { }
        public virtual void OnExceptionNotHandled(string ExceptionType, string Name, int Code, string Description, ref dbgExceptionAction ExceptionAction) { }
        public virtual void OnExceptionThrown(string ExceptionType, string Name, int Code, string Description, ref dbgExceptionAction ExceptionAction) { }

        //document event handlers
        public virtual void DocumentClosing(Document Document) { }
        public virtual void DocumentOpened(Document Document) { }
        public virtual void DocumentSaved(Document Document) { }

        //find event handlers
        public virtual void FindDone(vsFindResult Result, bool Cancelled) { }

        //misc file event handlers
        public virtual void ProjectItemAdded(ProjectItem ProjectItem) { }
        public virtual void ProjectItemRemoved(ProjectItem ProjectItem) { }
        public virtual void ProjectItemRenamed(ProjectItem ProjectItem, string OldName) { }

        //output window event handlers
        public virtual void OutputPaneUpdated(OutputWindowPane pPane) { }

        //selection event handlers
        public virtual void SelectionChange() { }

        //solution event handlers
        public virtual void SolutionBeforeClosing() { }
        public virtual void SolutionOpened() { }
        public virtual void ProjectAdded(Project Project) { }
        public virtual void SolutionRenamed(string OldName) { }

        //solution item event handlers
        public virtual void SolutionItemAdded(ProjectItem ProjectItem) { }
        public virtual void SolutionItemRemoved(ProjectItem ProjectItem) { }
        public virtual void SolutionItemRenamed(ProjectItem ProjectItem, string OldName) { }

        //text editor event handlers
        public virtual void EditorLineChanged(TextPoint StartPoint, TextPoint EndPoint, int Hint) { }
    }
}

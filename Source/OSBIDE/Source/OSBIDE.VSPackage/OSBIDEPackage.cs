using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using OSBIDE.VSPackage.WebServices;
using System.ServiceModel.Channels;
using System.ServiceModel;
using OSBIDE.Library;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using OSBIDE.Controls;
using System.Windows;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Windows.Threading;

namespace OSBIDE.VSPackage
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(OsbideStatusToolWindow))]
    [Guid(GuidList.guidOSBIDE_VSPackagePkgString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    public sealed class OSBIDEPackage : Package
    {
        private OsbideWebServiceClient webServiceClient = null;
        private OsbideEventHandler eventHandler = null;
        private OsbideContext localDb;
        public OsbideUser CurrentUser { get; private set; }
        private DispatcherTimer logSaveTimer = new DispatcherTimer();

        //Whether or not to allow the client to send web service calls.  This will get set to false if the
        //client and server libraries are out of sync or if we don't have a valid user.
        private bool allowLogServiceCalls = true;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public OSBIDEPackage()
        {
            CurrentUser = new OsbideUser();
            SqlCeConnection conn = new SqlCeConnection(StringConstants.LocalDataConnectionString);
            localDb = new OsbideContext(conn, true);
            logSaveTimer.Interval = new TimeSpan(0, 1, 0);

            //create our web service
            webServiceClient = new OsbideWebServiceClient(ServiceBindings.OsbideServiceBinding, ServiceBindings.OsbideServiceEndpoint);

            //and our event handler
            webServiceClient.SubmitLogCompleted += new EventHandler<SubmitLogCompletedEventArgs>(SubmitLogCompleted);
            logSaveTimer.Tick += new EventHandler(SaveLogs);
            logSaveTimer.Start();

            //check web service version number against ours
            CheckServiceVersion();
        }

        private void CheckServiceVersion()
        {
            string remoteVersionNumber = "";
            string packageUrl = "";

            //wrap web service calls in a try/catch just in case the endpoint can't be found
            try
            {
                remoteVersionNumber = webServiceClient.LibraryVersionNumber();
                packageUrl = webServiceClient.OsbidePackageUrl();
            }
            catch (Exception ex)
            {
                //write to the log file
                WriteToLog(string.Format("CheckServiceVersion error: {0}", ex.Message));

                //turn off future service calls for now
                allowLogServiceCalls = false;

                return;
            }

            //we have a version mismatch, stop sending data to the server
            if (StringConstants.LibraryVersion.CompareTo(remoteVersionNumber) != 0)
            {
                allowLogServiceCalls = false;
                UpdateAvailableWindow.ShowModalDialog(packageUrl);
            }
        }

        /// <summary>
        /// Writes the supplied text to OSBIDE's log file.
        /// </summary>
        /// <param name="content"></param>
        private void WriteToLog(string content)
        {
            using (StreamWriter writer = File.AppendText(StringConstants.LocalErrorLogPath))
            {
                writer.WriteLine("{0}:\t{1}", DateTime.Now.ToString("HH:mm:ss"), content);
            }
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(OsbideStatusToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            MessageBoxResult result = AccountWindow.ShowModalDialog(CurrentUser);

            //assume that data was changed and needs to be saved
            if (result == MessageBoxResult.OK)
            {
                CurrentUser = webServiceClient.SaveUser(CurrentUser);
                SaveUserData(CurrentUser);
                allowLogServiceCalls = true;
            }
        }

        private void SubmitLogCompleted(object sender, SubmitLogCompletedEventArgs e)
        {
            //were we not successful?
            if (e.Error != null)
            {
                WriteToLog(string.Format("SubmitLogCompleted error: {0}", e.Error.Message));
                return;
            }

            //The number coming back from the web service should be the ID number of the local log that
            //was saved.
            int logId = e.Result;

            EventLog log = localDb.EventLogs.Find(logId);

            if (log != null)
            {
                WriteToLog(string.Format("Removing log ID {0} from local DB", log.Id));
                localDb.Entry(log).State = System.Data.EntityState.Deleted;
                localDb.SaveChanges();
            }
        }

        private void SaveLogs(object sender, EventArgs e)
        {
            if (allowLogServiceCalls)
            {

                //find all logs that haven't been handled (submitted)
                List<EventLog> logs = localDb.EventLogs.Where(model => model.Handled == false).ToList();

                //send all unsubmitted logs to the server
                foreach (EventLog log in logs)
                {
                    try
                    {
                        //AC: EF attaches a bunch of crap to POCO objects for change tracking.  Said additions
                        //ruin WCF transfers.  There are supposidly fixes (see http://msdn.microsoft.com/en-us/library/dd456853.aspx)
                        //but for now, I'm just being lazy and using copy constructors to convert back to 
                        //standard objects.
                        EventLog cleanLog = new EventLog(log); //who doesn't like a clean log? :)
                        
                        //reset the log's sending user just to be safe
                        cleanLog.SenderId = 0;
                        cleanLog.Sender = CurrentUser;
                        WriteToLog(string.Format("Sending log with ID {0} to the server", cleanLog.Id));
                        webServiceClient.SubmitLogAsync(cleanLog);
                    }
                    catch (Exception ex)
                    {
                        WriteToLog(string.Format("SaveLogs error: {0}", ex.Message));
                    }
                }
            }
        }

        /// <summary>
        /// Called whenever OSBIDE detects an event change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OsbideEventCreated(object sender, EventCreatedArgs e)
        {
            //add the log to the local db
            EventLog eventLog = new EventLog(e.OsbideEvent, CurrentUser);
            localDb.EventLogs.Add(eventLog);
            localDb.SaveChanges();
            WriteToLog(string.Format("Event of type {0} created and saved to DB", e.OsbideEvent.GetType().Name));

            //The method "SaveLogs" takes care of the actual saving.  It is triggered periodically by
            //a DispatcherTimer.  As DispatcherTimer's aren't true multi-threading, we shouldn't have 
            //any race conditions (hopefully?)
        }

        /// <summary>
        /// Retrieves saved user data
        /// </summary>
        /// <returns></returns>
        private OsbideUser GetSavedUserData()
        {
            return OsbideUser.ReadUserFromFile(StringConstants.UserDataPath);
        }

        /// <summary>
        /// Saves current user data to disk
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private bool SaveUserData(OsbideUser user)
        {
            OsbideUser.SaveUserToFile(user, StringConstants.UserDataPath);
            return true;
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            //AC: Explicity load in assemblies.  Necessary for serialization (why?)
            Assembly.Load("OSBIDE.Library");
            Assembly.Load("OSBIDE.Controls");

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidOSBIDE_VSPackageCmdSet, (int)PkgCmdIDList.cmdidOsbideCommand);
                MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
                mcs.AddCommand(menuItem);
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidList.guidOSBIDE_VSPackageCmdSet, (int)PkgCmdIDList.cmdidOsbideStatusTool);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand(menuToolWin);
            }

            //attach event listeners to user actions
            eventHandler = new OsbideEventHandler(this as System.IServiceProvider);
            eventHandler.EventCreated += new EventHandler<EventCreatedArgs>(OsbideEventCreated);

            //pull saved user data
            CurrentUser = GetSavedUserData();

            //display a user notification if we don't have any user on file
            if (CurrentUser.Id == 0)
            {
                allowLogServiceCalls = false;
                MessageBoxResult result = MessageBox.Show("Thank you for installing OSBIDE.  To complete the installation, you must enter your user information.  Would you like to do this now?  You can always make changes to your information by clicking on the \"Tools\" menu and selecting \"OSBIDE\".", "Welcome to OSBIDE", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    MenuItemCallback(this, EventArgs.Empty);
                }
            }
        }

        #endregion

    }
}

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
using OSBIDE.Controls.Views;
using OSBIDE.Controls.ViewModels;
using EnvDTE80;
using OSBIDE.Library.Logging;
using OSBIDE.Library.ServiceClient;

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
    [ProvideAutoLoad(UIContextGuids80.NoSolution)]
    public sealed class OSBIDEPackage : Package
    {
        private OsbideWebServiceClient _webServiceClient = null;
        private bool _hasWebServiceError = false;
        private OsbideEventHandler _eventHandler = null;
        private ILogger _errorLogger = new LocalErrorLogger();
        public OsbideUser CurrentUser { get; private set; }
        private ServiceClient _client;
        private OsbideContext _db;

        //If OSBIDE isn't up to date, don't allow logging as it means that we've potentially 
        //changed the way the web service operates
        private bool _isOsbideUpToDate = true;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public OSBIDEPackage()
        {
            //AC: For consolidation purposes, I've just thrown everything inside the Initialize method.
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
            if (_db != null)
            {
                AssignmentSubmissionsViewModel avm = new AssignmentSubmissionsViewModel(_db);
                OsbideStatusViewModel vm = new OsbideStatusViewModel();
                vm.SubmissionViewModel = avm;
                vm.StatusViewModel = new TransmissionStatusViewModel();
                (window.Content as OsbideStatus).ViewModel = vm;
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
                try
                {
                    CurrentUser = _webServiceClient.SaveUser(CurrentUser);
                }
                catch (Exception ex)
                {
                    //write to the log file
                    _errorLogger.WriteToLog(string.Format("SaveUser error: {0}", ex.Message), LogPriority.HighPriority);

                    //turn off future service calls for now
                    _isOsbideUpToDate = false;
                }

                //If we got back a valid user, turn on log saving
                if (CurrentUser.Id != 0)
                {
                    SaveUserData(CurrentUser);
                    _isOsbideUpToDate = true;
                }
                else
                {
                    _isOsbideUpToDate = false;
                }
            }
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
                
                //create submit assignment command
                //(turned off for fall study))
                /*
                CommandID submitCommand = new CommandID(GuidList.guidOSBIDE_VSPackageCmdSet, (int)PkgCmdIDList.cmdidOsbideSubmitAssignmentCommand);
                MenuCommand submitMenuItem = new MenuCommand(SubmitAssignmentCallback, submitCommand);
                mcs.AddCommand(submitMenuItem);
                */

                // Create the command for the tool window
                //(turned off for fall study)
                /*
                CommandID toolwndCommandID = new CommandID(GuidList.guidOSBIDE_VSPackageCmdSet, (int)PkgCmdIDList.cmdidOsbideStatusTool);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand(menuToolWin);
                 * */
            }

            //create our web service
            _webServiceClient = new OsbideWebServiceClient(ServiceBindings.OsbideServiceBinding, ServiceBindings.OsbideServiceEndpoint);

            //pull saved user data
            CurrentUser = GetSavedUserData();

            //display a user notification if we don't have any user on file
            if (CurrentUser.Id == 0)
            {
                _isOsbideUpToDate = false;
                MessageBoxResult result = MessageBox.Show("Thank you for installing OSBIDE.  To complete the installation, you must enter your user information.  Would you like to do this now?  You can always make changes to your information by clicking on the \"Tools\" menu and selecting \"OSBIDE\".", "Welcome to OSBIDE", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    MenuItemCallback(this, EventArgs.Empty);
                }
            }
            else
            {
                //make sure that the saved user has the right ID
                OsbideUser webUser = null;
                try
                {
                    webUser = _webServiceClient.GetUserById(CurrentUser.Id);
                }
                catch (EndpointNotFoundException notFoundException)
                {
                    _errorLogger.WriteToLog("Web service error: " + notFoundException.Message, LogPriority.HighPriority);
                    _hasWebServiceError = true;
                }
                if (webUser == null && _hasWebServiceError == false)
                {
                    CurrentUser = _webServiceClient.SaveUser(CurrentUser);
                    SaveUserData(CurrentUser);
                }
            }

            //check web service version number against ours
            CheckServiceVersion();

            /* AC: SQL Server CE is not needed for the Fall study, so disable all
             * checks.
            if (!OsbideContext.HasSqlServerCE)
            {
                _errorLogger.WriteToLog("SQL Server CE not detected.  Prompting user to install...", LogPriority.MediumPriority);
                MessageBoxResult result = MessageBox.Show("OSBIDE requires SQL Server CE in order to properly function.  Would you like to download this now?",
                                            "Missing Component",
                                            MessageBoxButton.YesNo
                                            );
                if (result == MessageBoxResult.Yes)
                {

                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("http://www.microsoft.com/download/en/details.aspx?id=17876"));
                }
            }
            else
            {
                SqlCeConnection conn = new SqlCeConnection(StringConstants.LocalDataConnectionString);
                _db = new OsbideContext(conn, true);
            }
             * */

            if (_isOsbideUpToDate)
            {
                _eventHandler = new OsbideEventHandler(this as System.IServiceProvider, EventGenerator.GetInstance());
                _client = ServiceClient.GetInstance(_eventHandler, _errorLogger);
            }            
        }

        #endregion

        #region AC Code
        private void CheckServiceVersion()
        {
            string remoteVersionNumber = "";
            string packageUrl = "";

            //wrap web service calls in a try/catch just in case the endpoint can't be found
            try
            {
                remoteVersionNumber = _webServiceClient.LibraryVersionNumber();
                packageUrl = _webServiceClient.OsbidePackageUrl();
            }
            catch (Exception ex)
            {
                //write to the log file
                _errorLogger.WriteToLog(string.Format("CheckServiceVersion error: {0}", ex.Message), LogPriority.HighPriority);

                //turn off future service calls for now
                _isOsbideUpToDate = false;

                return;
            }

            //we have a version mismatch, stop sending data to the server & delete localDb
            if (StringConstants.LibraryVersion.CompareTo(remoteVersionNumber) != 0)
            {
                _isOsbideUpToDate = false;
                File.Delete(StringConstants.LocalDatabasePath);
                UpdateAvailableWindow.ShowModalDialog(packageUrl);
            }
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

        /// <summary>
        /// Called when the user selects the "submit assignment" menu option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubmitAssignmentCallback(object sender, EventArgs e)
        {
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            SubmitEvent evt = new SubmitEvent();
            DTE2 dte = (DTE2)this.GetService(typeof(SDTE));

            if (dte.Solution.FullName.Length == 0)
            {
                MessageBox.Show("No solution is currently open.");
                return;
            }

            evt.SolutionName = dte.Solution.FullName;
            
            SubmitAssignmentViewModel vm = new SubmitAssignmentViewModel(CurrentUser, evt);
            MessageBoxResult result = SubmitAssignmentWindow.ShowModalDialog(vm);
            
            //assume that data was changed and needs to be saved
            if (result == MessageBoxResult.OK)
            {
                EventGenerator generator = EventGenerator.GetInstance();
                generator.RequestSolutionSubmit(CurrentUser, vm.SelectedAssignment);    

            }
        }
        #endregion

    }
}

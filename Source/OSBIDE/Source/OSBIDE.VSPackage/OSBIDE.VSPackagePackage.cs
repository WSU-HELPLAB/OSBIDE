using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using OSBIDE.Controls.WebServices;
using OSBIDE.Library.Events;
using OSBIDE.Library.Logging;
using OSBIDE.Library.Models;
using OSBIDE.Library.ServiceClient;
using System.Reflection;
using OSBIDE.Controls;
using OSBIDE.Controls.ViewModels;
using OSBIDE.Controls.Views;
using System.Windows;
using System.ServiceModel;
using EnvDTE80;
using OSBIDE.Library;
using System.IO;
using System.Runtime.Caching;
using EnvDTE;

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
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(ActivityFeedToolWindow))]
    [ProvideToolWindow(typeof(ActivityFeedDetailsToolWindow))]
    [ProvideToolWindow(typeof(ChatToolWindow))]
    [ProvideToolWindow(typeof(UserProfileToolWindow))]
    [ProvideToolWindow(typeof(CreateAccountToolWindow))]
    [ProvideToolWindow(typeof(AskTheProfessorToolWindow))]
    [ProvideAutoLoad(UIContextGuids80.NoSolution)]
    [Guid(GuidList.guidOSBIDE_VSPackagePkgString)]
    public sealed class OSBIDE_VSPackagePackage : Package, IDisposable
    {
        private OsbideWebServiceClient _webServiceClient = null;
        private bool _hasWebServiceError = false;
        private OsbideEventHandler _eventHandler = null;
        private ILogger _errorLogger = new LocalErrorLogger();
        private ServiceClient _client;
        private OsbideContext _db;
        private ObjectCache _cache = new FileCache(StringConstants.LocalCacheDirectory, new LibraryBinder());
        private string _userName = null;
        private string _userPassword = null;
        private string _webServiceKey = null;
        private OsbideToolWindowManager _manager = null;

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
        public OSBIDE_VSPackagePackage()
        {
            //AC: For consolidation purposes, I've just thrown everything inside the Initialize method.
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

                //activity feed
                CommandID activityFeedId = new CommandID(GuidList.guidOSBIDE_VSPackageCmdSet, (int)PkgCmdIDList.cmdidOsbideActivityFeedTool);
                MenuCommand menuActivityWin = new MenuCommand(ShowActivityFeedTool, activityFeedId);
                mcs.AddCommand(menuActivityWin);

                //activity feed details
                CommandID activityFeedDetailsId = new CommandID(GuidList.guidOSBIDE_VSPackageCmdSet, (int)PkgCmdIDList.cmdidOsbideActivityFeedDetailsTool);
                MenuCommand menuActivityDetailsWin = new MenuCommand(ShowActivityFeedDetailsTool, activityFeedDetailsId);
                mcs.AddCommand(menuActivityDetailsWin);

                //chat window
                CommandID chatWindowId = new CommandID(GuidList.guidOSBIDE_VSPackageCmdSet, (int)PkgCmdIDList.cmdidOsbideChatTool);
                MenuCommand menuChatWin = new MenuCommand(ShowChatTool, chatWindowId);
                mcs.AddCommand(menuChatWin);

                //profile page
                CommandID profileWindowId = new CommandID(GuidList.guidOSBIDE_VSPackageCmdSet, (int)PkgCmdIDList.cmdidOsbideUserProfileTool);
                MenuCommand menuProfileWin = new MenuCommand(ShowProfileTool, profileWindowId);
                mcs.AddCommand(menuProfileWin);

                //"ask for help context" menu
                CommandID askForHelpId = new CommandID(GuidList.guidOSBIDE_ContextMenuCmdSet, (int)PkgCmdIDList.cmdOsbideAskForHelp);
                OleMenuCommand askForHelpWin = new OleMenuCommand(ShowAskForHelp, askForHelpId);
                askForHelpWin.BeforeQueryStatus += AskForHelpCheckActive;
                mcs.AddCommand(askForHelpWin);

                //create account window
                CommandID createAccountWindowId = new CommandID(GuidList.guidOSBIDE_VSPackageCmdSet, (int)PkgCmdIDList.cmdidOsbideCreateAccountTool);
                MenuCommand menuAccountWin = new MenuCommand(ShowCreateAccountTool, createAccountWindowId);
                mcs.AddCommand(menuAccountWin);

                //submit assignment command
                CommandID submitCommand = new CommandID(GuidList.guidOSBIDE_VSPackageCmdSet, (int)PkgCmdIDList.cmdidOsbideSubmitAssignmentCommand);
                MenuCommand submitMenuItem = new MenuCommand(SubmitAssignmentCallback, submitCommand);
                mcs.AddCommand(submitMenuItem);

                //ask the professor window
                CommandID askProfessorWindowId = new CommandID(GuidList.guidOSBIDE_VSPackageCmdSet, (int)PkgCmdIDList.cmdidOsbideAskTheProfessor);
                MenuCommand menuAskProfessorWin = new MenuCommand(ShowAskProfessorTool, askProfessorWindowId);
                mcs.AddCommand(menuAskProfessorWin);

            }

            //create our web service
            _webServiceClient = new OsbideWebServiceClient(ServiceBindings.OsbideServiceBinding, ServiceBindings.OsbideServiceEndpoint);

            //pull saved user data
            _userName = _cache[StringConstants.UserNameCacheKey] as string;
            _userPassword = _cache[StringConstants.PasswordCacheKey] as string;

            //display a user notification if we don't have any user on file
            if (_userName == null || _userPassword == null)
            {
                _isOsbideUpToDate = false;
                MessageBoxResult result = MessageBox.Show("Thank you for installing OSBIDE.  To complete the installation, you must enter your user information, which can be done by clicking on the \"Tools\" menu and selecting \"Log into OSBIDE\".", "Welcome to OSBIDE", MessageBoxButton.OK);
            }
            else
            {
                string authKey = "";
                try
                {
                    string hashedPassword = UserPassword.EncryptPassword(_userPassword, _userName);
                    authKey = _webServiceClient.Login(_userName, hashedPassword);
                    if (authKey.Length == 0)
                    {
                        MessageBoxResult result = MessageBox.Show("It appears as though your OSBIDE user name or password has changed since the last time you opened Visual Studio.  Would you like to log back into OSBIDE?", "Log Into OSBIDE", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            MenuItemCallback(this, EventArgs.Empty);
                        }
                    }
                }
                catch (EndpointNotFoundException notFoundException)
                {
                    _errorLogger.WriteToLog("Web service error: " + notFoundException.Message, LogPriority.HighPriority);
                    _hasWebServiceError = true;
                }
                catch (Exception ex)
                {
                    _errorLogger.WriteToLog("Web service error: " + ex.Message, LogPriority.HighPriority);
                    _hasWebServiceError = true;
                }
                if (authKey.Length > 0)
                {
                    _cache[StringConstants.AuthenticationCacheKey] = authKey;
                }
            }

            //check web service version number against ours
            CheckServiceVersion();

            if (_isOsbideUpToDate)
            {
                _eventHandler = new OsbideEventHandler(this as System.IServiceProvider, EventGenerator.GetInstance());
                _client = ServiceClient.GetInstance(_eventHandler, _errorLogger);
            }

            _manager = new OsbideToolWindowManager(_cache as FileCache, this);
        }
        #endregion

        #region AC Code

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            /*
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
             * */
        }

        private void ShowAwesomiumError()
        {
            MessageBox.Show("It appears as though your system is missing prerequisite components necessary for OSBIDE to operate properly.  Until this is resolved, you will not be able to access certain OSBIDE components within Visual Studio.  You can download the prerequisite files and obtain support by visiting http://osbide.codeplex.com.", "OSBIDE", MessageBoxButton.OK);
        }

        private void ShowActivityFeedTool(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_cache[StringConstants.AuthenticationCacheKey].ToString()) == false)
            {
                try
                {
                    _manager.OpenActivityFeedWindow();
                }
                catch (Exception)
                {
                    ShowAwesomiumError();
                }
            }
            else
            {
                MessageBox.Show("You must be logged into OSBIDE in order to access this window.");
            }
        }

        private void ShowActivityFeedDetailsTool(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_cache[StringConstants.AuthenticationCacheKey].ToString()) == false)
            {
                try
                {
                    _manager.OpenActivityFeedDetailsWindow();
                }
                catch (Exception)
                {
                    ShowAwesomiumError();
                }
            }
            else
            {
                MessageBox.Show("You must be logged into OSBIDE in order to access this window.");
            }
        }

        private void ShowChatTool(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_cache[StringConstants.AuthenticationCacheKey].ToString()) == false)
            {
                try
                {
                    _manager.OpenChatWindow();
                }
                catch (Exception)
                {
                    ShowAwesomiumError();
                }
            }
            else
            {
                MessageBox.Show("You must be logged into OSBIDE in order to access this window.");
            }
        }

        private void ShowProfileTool(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_cache[StringConstants.AuthenticationCacheKey].ToString()) == false)
            {
                try
                {
                    _manager.OpenProfileWindow();
                }
                catch (Exception)
                {
                    ShowAwesomiumError();
                }
            }
            else
            {
                MessageBox.Show("You must be logged into OSBIDE in order to access this window.");
            }
        }

        private void ShowCreateAccountTool(object sender, EventArgs e)
        {
            try
            {
                _manager.OpenCreateAccountWindow();
            }
            catch (Exception)
            {
                ShowAwesomiumError();
            }

        }

        private void ShowAskProfessorTool(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_cache[StringConstants.AuthenticationCacheKey].ToString()) == false)
            {
                try
                {
                    _manager.OpenAskTheProfessorWindow();
                }
                catch (Exception)
                {
                    ShowAwesomiumError();
                }
            }
            else
            {
                MessageBox.Show("You must be logged into OSBIDE in order to access this window.");
            }
        }

        public void ShowAskForHelp(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_cache[StringConstants.AuthenticationCacheKey].ToString()) == true)
            {
                MessageBox.Show("You must be logged into OSBIDE in order to access this window.");
                return;
            }

            AskForHelpViewModel vm = new AskForHelpViewModel();

            //find current text selection
            DTE2 dte = (DTE2)this.GetService(typeof(SDTE));
            if (dte != null)
            {
                TextSelection selection = dte.ActiveDocument.Selection as TextSelection;
                if (selection != null)
                {
                    vm.Code = selection.Text;
                }
            }

            //show message dialog
            MessageBoxResult result = AskForHelpForm.ShowModalDialog(vm);
            if (result == MessageBoxResult.OK)
            {
                EventGenerator generator = EventGenerator.GetInstance();
                AskForHelpEvent evt = new AskForHelpEvent();
                evt.Code = vm.Code;
                evt.UserComment = vm.UserText;
                generator.SubmitEvent(evt);
                MessageBox.Show("Your question has been logged and will show up in the activity stream shortly.");
            }
        }

        private void AskForHelpCheckActive(object sender, EventArgs e)
        {
            var cmd = sender as OleMenuCommand;
            DTE2 dte = (DTE2)this.GetService(typeof(SDTE));
            if (dte != null)
            {
                TextSelection selection = dte.ActiveDocument.Selection as TextSelection;
                if (selection != null)
                {
                    string text = selection.Text;
                    if (string.IsNullOrEmpty(text) == true)
                    {
                        cmd.Enabled = false;
                    }
                    else
                    {
                        cmd.Enabled = true;
                    }
                }
            }
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
            OsbideLoginViewModel vm = new OsbideLoginViewModel();
            vm.RequestCreateAccount += ShowCreateAccountTool;

            //attempt to store previously cached values if possible
            vm.Password = _userPassword;
            vm.Email = _userName;

            MessageBoxResult result = OsbideLoginControl.ShowModalDialog(vm);

            //assume that data was changed and needs to be saved
            if (result == MessageBoxResult.OK)
            {
                try
                {
                    _cache[StringConstants.UserNameCacheKey] = vm.Email;
                    _userName = vm.Email;

                    _cache[StringConstants.PasswordCacheKey] = vm.Password;
                    _userPassword = vm.Password;

                    _cache[StringConstants.AuthenticationCacheKey] = vm.AuthenticationHash;

                }
                catch (Exception ex)
                {
                    //write to the log file
                    _errorLogger.WriteToLog(string.Format("SaveUser error: {0}", ex.Message), LogPriority.HighPriority);

                    //turn off future service calls for now
                    _isOsbideUpToDate = false;
                }

                //If we got back a valid user, turn on log saving
                if (_userName != null && _userPassword != null)
                {
                    _isOsbideUpToDate = true;

                    MessageBox.Show("Welcome to OSBIDE!");
                }
                else
                {
                    _isOsbideUpToDate = false;
                }
            }
        }

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

            SubmitAssignmentViewModel vm = new SubmitAssignmentViewModel(_cache[StringConstants.UserNameCacheKey] as string, evt);
            MessageBoxResult result = SubmitAssignmentWindow.ShowModalDialog(vm);

            //assume that data was changed and needs to be saved
            if (result == MessageBoxResult.OK)
            {
                EventGenerator generator = EventGenerator.GetInstance();
                generator.RequestSolutionSubmit(vm.SelectedAssignment);

            }
        }
        #endregion

        public void Dispose()
        {
            _webServiceClient.Close();
        }
    }
}

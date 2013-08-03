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
using OSBIDE.Plugins.Base;
using OSBIDE.Controls.WebServices;
using OSBIDE.Library.Events;
using OSBIDE.Library.Logging;
using OSBIDE.Library.ServiceClient;
using OSBIDE.Library.Models;
using System.Reflection;
using System.Runtime.Caching;
using OSBIDE.Library;
using System.Windows;
using OSBIDE.Controls.ViewModels;
using EnvDTE80;
using System.Windows.Documents;
using OSBIDE.Controls.Views;
using System.IO;
using OSBIDE.Controls;

namespace OSBIDE.Plugins.VS2012
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
    [ProvideToolWindow(typeof(ActivityFeedToolWindow), Style = VsDockStyle.Tabbed, Window = "3ae79031-e1bc-11d0-8f78-00a0c9110057")]
    [ProvideToolWindow(typeof(ActivityFeedDetailsToolWindow), Style = VsDockStyle.MDI, MultiInstances = true)]
    [ProvideToolWindow(typeof(ChatToolWindow), Style = VsDockStyle.MDI)]
    [ProvideToolWindow(typeof(UserProfileToolWindow), Style = VsDockStyle.MDI)]
    [ProvideToolWindow(typeof(CreateAccountToolWindow), Style = VsDockStyle.MDI)]
    //[ProvideToolWindow(typeof(AskTheProfessorToolWindow))]
    [ProvideAutoLoad(UIContextGuids80.NoSolution)]
    [Guid(CommonGuidList.guidOSBIDE_VSPackagePkgString)]
    public sealed class OSBIDE_Plugins_VS2012Package : Package, IDisposable, IVsShellPropertyEvents
    {
        private OsbideWebServiceClient _webServiceClient = null;
        private bool _hasWebServiceError = false;
        private OsbideEventHandler _eventHandler = null;
        private ILogger _errorLogger = new LocalErrorLogger();
        private ServiceClient _client;
        private OsbideContext _db;
        private FileCache _cache = Cache.CacheInstance;
        private string _userName = null;
        private string _userPassword = null;
        private string _webServiceKey = null;
        private OsbideToolWindowManager _manager = null;
        private uint _EventSinkCookie;

        //If OSBIDE isn't up to date, don't allow logging as it means that we've potentially 
        //changed the way the web service operates
        private bool _isOsbideUpToDate = true;

        private bool _hasStartupErrors = false;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public OSBIDE_Plugins_VS2012Package()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }


        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            //AC: Explicity load in assemblies.  Necessary for serialization (why?)
            Assembly.Load("OSBIDE.Library");
            Assembly.Load("OSBIDE.Controls");

            //force load awesomium dlls
            /*
            try
            {
                Assembly.Load("Awesomium.Core, Version=1.7.1.0, Culture=neutral, PublicKeyToken=e1a0d7c8071a5214");
                Assembly.Load("Awesomium.Windows.Controls, Version=1.7.1.0, Culture=neutral, PublicKeyToken=7a34e179b8b61c39");
            }
            catch (Exception ex)
            {
                _errorLogger.WriteToLog("Error loading awesomium DLLs: " + ex.Message, LogPriority.HighPriority);
            }
             * */

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                //login toolbar item.
                CommandID menuCommandID = new CommandID(CommonGuidList.guidOSBIDE_VSPackageCmdSet, (int)CommonPkgCmdIDList.cmdidOsbideCommand);
                MenuCommand menuItem = new MenuCommand(OpenLoginScreen, menuCommandID);
                mcs.AddCommand(menuItem);

                //login toolbar menu option.
                CommandID loginMenuOption = new CommandID(CommonGuidList.guidOSBIDE_OsbideToolsMenuCmdSet, (int)CommonPkgCmdIDList.cmdidOsbideLoginToolWin);
                MenuCommand menuLoginMenuOption = new MenuCommand(OpenLoginScreen, loginMenuOption);
                mcs.AddCommand(menuLoginMenuOption);

                //activity feed
                CommandID activityFeedId = new CommandID(CommonGuidList.guidOSBIDE_VSPackageCmdSet, (int)CommonPkgCmdIDList.cmdidOsbideActivityFeedTool);
                MenuCommand menuActivityWin = new MenuCommand(ShowActivityFeedTool, activityFeedId);
                mcs.AddCommand(menuActivityWin);

                //activity feed details
                CommandID activityFeedDetailsId = new CommandID(CommonGuidList.guidOSBIDE_VSPackageCmdSet, (int)CommonPkgCmdIDList.cmdidOsbideActivityFeedDetailsTool);
                MenuCommand menuActivityDetailsWin = new MenuCommand(ShowActivityFeedDetailsTool, activityFeedDetailsId);
                mcs.AddCommand(menuActivityDetailsWin);

                //chat window
                CommandID chatWindowId = new CommandID(CommonGuidList.guidOSBIDE_VSPackageCmdSet, (int)CommonPkgCmdIDList.cmdidOsbideChatTool);
                MenuCommand menuChatWin = new MenuCommand(ShowChatTool, chatWindowId);
                mcs.AddCommand(menuChatWin);

                //profile page
                CommandID profileWindowId = new CommandID(CommonGuidList.guidOSBIDE_VSPackageCmdSet, (int)CommonPkgCmdIDList.cmdidOsbideUserProfileTool);
                MenuCommand menuProfileWin = new MenuCommand(ShowProfileTool, profileWindowId);
                mcs.AddCommand(menuProfileWin);

                //"ask for help context" menu
                CommandID askForHelpId = new CommandID(CommonGuidList.guidOSBIDE_ContextMenuCmdSet, (int)CommonPkgCmdIDList.cmdOsbideAskForHelp);
                OleMenuCommand askForHelpWin = new OleMenuCommand(ShowAskForHelp, askForHelpId);
                askForHelpWin.BeforeQueryStatus += AskForHelpCheckActive;
                mcs.AddCommand(askForHelpWin);

                //create account window
                CommandID createAccountWindowId = new CommandID(CommonGuidList.guidOSBIDE_VSPackageCmdSet, (int)CommonPkgCmdIDList.cmdidOsbideCreateAccountTool);
                MenuCommand menuAccountWin = new MenuCommand(ShowCreateAccountTool, createAccountWindowId);
                mcs.AddCommand(menuAccountWin);

                //submit assignment command
                //(commented out for Fall 2013 release at instructor request)
                /*
                CommandID submitCommand = new CommandID(GuidList.guidOSBIDE_VSPackageCmdSet, (int)PkgCmdIDList.cmdidOsbideSubmitAssignmentCommand);
                MenuCommand submitMenuItem = new MenuCommand(SubmitAssignmentCallback, submitCommand);
                mcs.AddCommand(submitMenuItem);
                */

                //ask the professor window 
                //(commented out for Fall 2013 release at instructor request)
                /*
                CommandID askProfessorWindowId = new CommandID(GuidList.guidOSBIDE_VSPackageCmdSet, (int)PkgCmdIDList.cmdidOsbideAskTheProfessor);
                MenuCommand menuAskProfessorWin = new MenuCommand(ShowAskProfessorTool, askProfessorWindowId);
                mcs.AddCommand(menuAskProfessorWin);
                 * */

                // -- Set an event listener for shell property changes
                var shellService = GetService(typeof(SVsShell)) as IVsShell;
                if (shellService != null)
                {
                    ErrorHandler.ThrowOnFailure(shellService.
                      AdviseShellPropertyChanges(this, out _EventSinkCookie));
                } 

            }

            //create our web service
            _webServiceClient = new OsbideWebServiceClient(ServiceBindings.OsbideServiceBinding, ServiceBindings.OsbideServiceEndpoint);
            _webServiceClient.LibraryVersionNumberCompleted += InitStepThree_CheckServiceVersionComplete;
            _webServiceClient.LoginCompleted += InitStepTwo_LoginCompleted;

            //pull saved user data
            _userName = _cache[StringConstants.UserNameCacheKey] as string;
            _userPassword = _cache[StringConstants.PasswordCacheKey] as string;

            //set up tool window manager
            _manager = new OsbideToolWindowManager(_cache as FileCache, this);

            //set up service logger
            _eventHandler = new OsbideEventHandler(this as System.IServiceProvider, EventGenerator.GetInstance());
            _client = ServiceClient.GetInstance(_eventHandler, _errorLogger);
            _client.PropertyChanged += ServiceClientPropertyChanged;
            UpdateSendStatus();

            //display a user notification if we don't have any user on file
            if (_userName == null || _userPassword == null)
            {
                _hasStartupErrors = true;
                MessageBoxResult result = MessageBox.Show("Thank you for installing OSBIDE.  To complete the installation, you must enter your user information, which can be done by clicking on the \"Tools\" menu and selecting \"Log into OSBIDE\".", "Welcome to OSBIDE", MessageBoxButton.OK);
            }
            else
            {
                //step #1: attempt login
                string hashedPassword = UserPassword.EncryptPassword(_userPassword, _userName);

                try
                {
                    _webServiceClient.LoginAsync(_userName, hashedPassword);
                }
                catch (Exception ex)
                {
                    _errorLogger.WriteToLog("Web service error: " + ex.Message, LogPriority.HighPriority);
                    _hasStartupErrors = true;
                }
            }
            
        }
        #endregion



        #region AC Code

        /// <summary>
        /// The function continues the initialization process, picking up where Initialize() left off
        /// having called OSBIDE's login service
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InitStepTwo_LoginCompleted(object sender, LoginCompletedEventArgs e)
        {
            string authKey = "";
            if (e != null)
            {
                if (e.Result != null)
                {
                    authKey = e.Result;
                }
            }
            if (authKey.Length <= 0)
            {
                MessageBoxResult result = MessageBox.Show("It appears as though your OSBIDE user name or password has changed since the last time you opened Visual Studio.  Would you like to log back into OSBIDE?", "Log Into OSBIDE", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    OpenLoginScreen(this, EventArgs.Empty);
                }
            } else
            {
                _cache[StringConstants.AuthenticationCacheKey] = authKey;
            }

            //having logged in, we can now check to make sure we're up to date
            try
            {
                _webServiceClient.LibraryVersionNumberAsync();
            }
            catch (Exception ex)
            {
                //write to the log file
                _errorLogger.WriteToLog(string.Format("LibraryVersionNumberAsync error: {0}", ex.Message), LogPriority.HighPriority);
                _hasStartupErrors = true;
            }
        }

        /// <summary>
        /// This function continues the initialization process by picking up where InitStepTwo_LoginCompleted() left off.
        /// Namely, we assume that we've gone through the login process and have just received word if the local copy of
        /// OSBIDE is up to date.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void InitStepThree_CheckServiceVersionComplete(object sender, LibraryVersionNumberCompletedEventArgs e)
        {

            string remoteVersionNumber = "";

            if (e != null)
            {
                if (e.Result != null)
                {
                    remoteVersionNumber = e.Result;
                }
            }

            //if we have a version mismatch, stop sending data to the server & delete localDb
            if (StringConstants.LibraryVersion.CompareTo(remoteVersionNumber) != 0)
            {
                _isOsbideUpToDate = false;
                File.Delete(StringConstants.LocalDatabasePath);
                UpdateAvailableWindow.ShowModalDialog(StringConstants.OsbidePackageUrl);
            }

            //if we're all up to date and had no startup errors, then we can start sending logs to the server
            if (_isOsbideUpToDate == true && _hasStartupErrors == false)
            {
                _client.StartSending();
            }
        }

        /// <summary>
        /// Used to determine client send status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ServiceClientPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

            UpdateSendStatus();
        }

        private void UpdateSendStatus()
        {
            var dte = GetService(typeof(SDTE)) as DTE2;
            var cbs = ((Microsoft.VisualStudio.CommandBars.CommandBars)dte.CommandBars);
            Microsoft.VisualStudio.CommandBars.CommandBar cb = cbs["OSBIDE Toolbar"];
            Microsoft.VisualStudio.CommandBars.CommandBarControl toolsControl = cb.Controls["Log into OSBIDE"];
            Microsoft.VisualStudio.CommandBars.CommandBarButton loginButton = toolsControl as Microsoft.VisualStudio.CommandBars.CommandBarButton;

            if (_client.IsSendingData == true)
            {
                loginButton.Picture = (stdole.StdPicture)IconConverter.GetIPictureDispFromImage(Resources.login_active);
                loginButton.TooltipText = "Connected to OSBIDE";
            }
            else
            {
                loginButton.Picture = (stdole.StdPicture)IconConverter.GetIPictureDispFromImage(Resources.login);
                loginButton.TooltipText = "Not connected to OSBIDE.  Click to log in.";
            }
        }

        public int OnShellPropertyChange(int propid, object propValue)
        {
            // --- We handle the event if zombie state changes from true to false
            if ((int)__VSSPROPID.VSSPROPID_Zombie == propid)
            {
                if ((bool)propValue == false)
                {
                    // --- Show the commandbar
                    var dte = GetService(typeof(SDTE)) as DTE2;
                    var cbs = ((Microsoft.VisualStudio.CommandBars.CommandBars)dte.CommandBars);
                    Microsoft.VisualStudio.CommandBars.CommandBar cb = cbs["OSBIDE Toolbar"];
                    cb.Visible = true;

                    // --- Unsubscribe from events
                    var shellService = GetService(typeof(SVsShell)) as IVsShell;
                    if (shellService != null)
                    {
                        ErrorHandler.ThrowOnFailure(shellService.
                          UnadviseShellPropertyChanges(_EventSinkCookie));
                    }
                    _EventSinkCookie = 0;
                }
            }
            return VSConstants.S_OK;
        }

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

        private void ShowAwesomiumError(Exception ex)
        {
            if (ex != null)
            {
                _errorLogger.WriteToLog("Awesomium Error: " + ex.Message, LogPriority.HighPriority);
            }
            MessageBox.Show("It appears as though your system is missing prerequisite components necessary for OSBIDE to operate properly.  Until this is resolved, you will not be able to access certain OSBIDE components within Visual Studio.  You can download the prerequisite files and obtain support by visiting http://osbide.codeplex.com.", "OSBIDE", MessageBoxButton.OK);
        }

        private void ShowActivityFeedTool(object sender, EventArgs e)
        {
            object cacheItem = _cache[StringConstants.AuthenticationCacheKey];
            if (cacheItem != null && string.IsNullOrEmpty(cacheItem.ToString()) == false)
            {
                try
                {
                    _manager.OpenActivityFeedWindow();
                }
                catch (Exception ex)
                {
                    ShowAwesomiumError(ex);
                }
            }
            else
            {
                MessageBox.Show("You must be logged into OSBIDE in order to access this window.");
            }
           
        }

        private void ShowActivityFeedDetailsTool(object sender, EventArgs e)
        {
            object cacheItem = _cache[StringConstants.AuthenticationCacheKey];
            if (cacheItem != null && string.IsNullOrEmpty(cacheItem.ToString()) == false)
            {
                try
                {
                    _manager.OpenActivityFeedDetailsWindow();
                }
                catch (Exception ex)
                {
                    ShowAwesomiumError(ex);
                }
            }
            else
            {
                MessageBox.Show("You must be logged into OSBIDE in order to access this window.");
            }
        }

        private void ShowChatTool(object sender, EventArgs e)
        {
            object cacheItem = _cache[StringConstants.AuthenticationCacheKey];
            if (cacheItem != null && string.IsNullOrEmpty(cacheItem.ToString()) == false)
            {
                try
                {
                    _manager.OpenChatWindow();
                }
                catch (Exception ex)
                {
                    ShowAwesomiumError(ex);
                }
            }
            else
            {
                MessageBox.Show("You must be logged into OSBIDE in order to access this window.");
            }
        }

        private void ShowProfileTool(object sender, EventArgs e)
        {
            object cacheItem = _cache[StringConstants.AuthenticationCacheKey];
            if (cacheItem != null && string.IsNullOrEmpty(cacheItem.ToString()) == false)
            {
                try
                {
                    _manager.OpenProfileWindow();
                }
                catch (Exception ex)
                {
                    ShowAwesomiumError(ex);
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
            catch (Exception ex)
            {
                ShowAwesomiumError(ex);
            }

        }

        private void ShowAskProfessorTool(object sender, EventArgs e)
        {
            object cacheItem = _cache[StringConstants.AuthenticationCacheKey];
            if (cacheItem != null && string.IsNullOrEmpty(cacheItem.ToString()) == false)
            {
                try
                {
                    _manager.OpenAskTheProfessorWindow();
                }
                catch (Exception ex)
                {
                    ShowAwesomiumError(ex);
                }
            }
            else
            {
                MessageBox.Show("You must be logged into OSBIDE in order to access this window.");
            }
        }

        public void ShowAskForHelp(object sender, EventArgs e)
        {
            object cacheItem = _cache[StringConstants.AuthenticationCacheKey];
            if (cacheItem != null && string.IsNullOrEmpty(cacheItem.ToString()) == false)
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

            //AC: Restrict "ask for help" to approx 20 lines
            if (vm.Code.Length > 750)
            {
                vm.Code = vm.Code.Substring(0, 750);
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
        /// </summary>
        private void OpenLoginScreen(object sender, EventArgs e)
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

                    //turn off client sending if we run into an error
                    if (_client != null)
                    {
                        _client.StopSending();
                    }
                }

                //If we got back a valid user, turn on log saving
                if (_userName != null && _userPassword != null)
                {
                    //turn on client sending
                    if (_client != null)
                    {
                        _client.StartSending();
                    }
                    MessageBox.Show("Welcome to OSBIDE!");
                }
                else
                {
                    //turn off client sending if the user didn't log in.
                    if (_client != null)
                    {
                        _client.StopSending();
                    }
                }
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

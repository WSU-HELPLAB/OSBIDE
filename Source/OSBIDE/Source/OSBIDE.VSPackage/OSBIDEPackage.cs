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
        private OsbideContext localDb = new OsbideContext("Data Source=osbide_local.sdf;Persist Security Info=False;");
        private OsbideUser activeUser = new OsbideUser();

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public OSBIDEPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
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
            // Show a Message Box to prove we were here
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            MessageBoxResult result = AccountWindow.ShowModalDialog(activeUser);
            
            //assume that data was changed and needs to be saved
            if (result == MessageBoxResult.OK)
            {
                SaveUserData(activeUser);
            }
        }

        /// <summary>
        /// Called whenever OSBIDE detects an event change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void eventHandler_EventCreated(object sender, EventCreatedArgs e)
        {
            Enums.ServiceCode result = (Enums.ServiceCode)webServiceClient.SubmitLog(e.OsbideEvent.EventName, EventFactory.ToZippedBinary(e.OsbideEvent));
        }

        /// <summary>
        /// Retrieves saved user data
        /// </summary>
        /// <returns></returns>
        private OsbideUser GetSavedUserData()
        {
            string userDataPath = FilePaths.UserDataPath;
            OsbideUser savedUser = new OsbideUser();
            BinaryFormatter formatter = new BinaryFormatter();

            if (File.Exists(userDataPath))
            {
                FileStream data = File.Open(userDataPath, FileMode.Open);
                try
                {
                    savedUser = (OsbideUser)formatter.Deserialize(data);
                }
                catch (Exception ex)
                {
                }
            }
            return savedUser;
        }

        /// <summary>
        /// Saves current user data to disk
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private bool SaveUserData(OsbideUser user)
        {
            string userDataPath = FilePaths.UserDataPath;
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream file = File.Open(userDataPath, FileMode.Create);
            formatter.Serialize(file, user);
            file.Close();
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
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

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

            //create our web service
            webServiceClient = new OsbideWebServiceClient(ServiceBindings.OsbideServiceBinding, ServiceBindings.OsbideServiceEndpoint);

            //and our event handler
            eventHandler = new OsbideEventHandler(this as System.IServiceProvider);
            eventHandler.EventCreated += new EventHandler<EventCreatedArgs>(eventHandler_EventCreated);

        }

        #endregion

    }
}

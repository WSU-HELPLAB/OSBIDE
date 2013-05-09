﻿using Microsoft.VisualStudio.Shell;
using OSBIDE.Controls.Views;
using System.Runtime.InteropServices;
namespace OSBIDE_VS2012Package
{
    [Guid("eee1c7ba-00ea-4b22-88d7-6cb17837c3d9")]
    public class CreateAccountToolWindow: ToolWindowPane
    {
        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public CreateAccountToolWindow () :
            base(null)
        {
            // Set the window title reading it from the resources.
            this.Caption = "Create Account";
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
            // the object returned by the Content property.
            base.Content = new BrowserView();

        }
    }
}

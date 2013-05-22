using Microsoft.VisualStudio.Shell;
using OSBIDE.Controls.ViewModels;
using OSBIDE.Controls.Views;
using OSBIDE.Library;
using System;
using System.Runtime.Caching;
using System.Runtime.InteropServices;
namespace OSBIDE.VSPackage
{
    [Guid("eee1c7ba-00ea-4b22-88d7-6cb17837c3d5")]
    public class ActivityFeedDetailsToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public ActivityFeedDetailsToolWindow() :
            base(null)
        {
            // Set the window title reading it from the resources.
            this.Caption = "Feed Details";
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
            BrowserView view = new BrowserView();
            FileCache cache = OSBIDE_VSPackagePackage.CacheInstance;
            string url = "";
            try
            {
                url = cache[OsbideVsComponent.FeedDetails.ToString()].ToString();
            }
            catch (Exception)
            {
                url = "";
            }
            view.ViewModel = new Controls.ViewModels.BrowserViewModel()
            {
                Url = url,
                AuthKey = cache[StringConstants.AuthenticationCacheKey].ToString()
            };
            base.Content = view;
        }
    }
}

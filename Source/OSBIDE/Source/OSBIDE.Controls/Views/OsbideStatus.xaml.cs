using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OSBIDE.Controls.ViewModels;

namespace OSBIDE.Controls.Views
{
    /// <summary>
    /// Interaction logic for OsbideStatus.xaml
    /// </summary>
    public partial class OsbideStatus : UserControl
    {
        public OsbideStatusViewModel ViewModel
        {
            get
            {
                return this.DataContext as OsbideStatusViewModel;
            }
            set
            {
                this.DataContext = value;

                //AC: this shouldn't be necessary.  Todo: figure out why this won't
                //happen automatically
                /*AC: turned off for fall study
                TransmissionTab.ViewModel = ViewModel.StatusViewModel;
                SubmissionsTab.ViewModel = ViewModel.SubmissionViewModel;
                 * */
            }
        }

        public OsbideStatus()
        {
            InitializeComponent();

            this.ViewModel = new OsbideStatusViewModel();
        }
    }
}

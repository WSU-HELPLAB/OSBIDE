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
    /// Interaction logic for AssignmentSubmissions.xaml
    /// </summary>
    public partial class AssignmentSubmissions : UserControl
    {

        public AssignmentSubmissionsViewModel ViewModel
        {
            get 
            {
                return this.DataContext as AssignmentSubmissionsViewModel; 
            }
            set 
            { 
                this.DataContext = value;
            }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(AssignmentSubmissionsViewModel), typeof(AssignmentSubmissions), new UIPropertyMetadata(new AssignmentSubmissionsViewModel()));
        

        public AssignmentSubmissions()
        {
            InitializeComponent();
        }
    }
}

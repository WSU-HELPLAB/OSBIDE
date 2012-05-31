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

        public AssignmentSubmissions()
        {
            InitializeComponent();
        }
    }
}

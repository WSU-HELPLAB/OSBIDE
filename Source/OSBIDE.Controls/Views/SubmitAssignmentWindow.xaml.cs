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
using System.Windows.Shapes;
using OSBIDE.Controls.ViewModels;

namespace OSBIDE.Controls.Views
{
    /// <summary>
    /// Interaction logic for SubmitAssignmentWindow.xaml
    /// </summary>
    public partial class SubmitAssignmentWindow : Window
    {
        public SubmitAssignmentViewModel ViewModel
        {
            get
            {
                return this.DataContext as SubmitAssignmentViewModel;
            }
            set
            {
                if (ViewModel != null)
                {
                    ViewModel.RequestClose -= new EventHandler(ViewModel_RequestClose);
                }
                this.DataContext = value;
                ViewModel.RequestClose += new EventHandler(ViewModel_RequestClose);
            }
        }

        void ViewModel_RequestClose(object sender, EventArgs e)
        {
            this.Close();
        }

        public SubmitAssignmentWindow()
        {
            InitializeComponent();
        }

        public static MessageBoxResult ShowModalDialog(SubmitAssignmentViewModel vm)
        {
            SubmitAssignmentWindow window = new SubmitAssignmentWindow();
            window.ViewModel = vm;
            window.ShowDialog();
            return vm.Result;
        }
    }
}

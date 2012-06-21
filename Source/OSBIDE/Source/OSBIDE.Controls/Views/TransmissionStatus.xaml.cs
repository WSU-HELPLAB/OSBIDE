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
    /// Interaction logic for TransmissionStatus.xaml
    /// </summary>
    public partial class TransmissionStatus : UserControl
    {

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(TransmissionStatusViewModel), typeof(TransmissionStatus), new UIPropertyMetadata(new TransmissionStatusViewModel()));


        public TransmissionStatusViewModel ViewModel
        {
            get
            {
                return this.DataContext as TransmissionStatusViewModel;
            }
            set
            {
                this.DataContext = value;
            }
        }

        public TransmissionStatus()
        {
            InitializeComponent();
            ViewModel = new TransmissionStatusViewModel();
        }
    }
}

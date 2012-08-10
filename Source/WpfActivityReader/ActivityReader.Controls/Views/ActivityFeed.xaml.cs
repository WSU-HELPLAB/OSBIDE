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
using ActivityReader.Controls.ViewModels;

namespace ActivityReader.Controls.Views
{
    /// <summary>
    /// Interaction logic for ActivityFeed.xaml
    /// </summary>
    public partial class ActivityFeed : UserControl
    {
        public ActivityFeedViewModel ViewModel
        {
            get
            {
                return DataContext as ActivityFeedViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        public ActivityFeed()
        {
            InitializeComponent();
        }
    }
}

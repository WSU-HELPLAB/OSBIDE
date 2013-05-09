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
using OSBIDE.Library.Models;
using System.Diagnostics;
using OSBIDE.Library;
using System.Drawing;

namespace OSBIDE.Controls
{
    /// <summary>
    /// Interaction logic for AccountWindow.xaml
    /// </summary>
    public partial class AccountWindow : Window
    {
        private OsbideUser _activeUser;
        public OsbideUser ActiveUser
        {
            get
            {
                return _activeUser;
            }
            set
            {
                _activeUser = value;
                this.DataContext = _activeUser;
            }
        }
        public MessageBoxResult Result { get; private set; }
        public AccountWindow()
        {
            InitializeComponent();
            ActiveUser = new OsbideUser();

            //bind to our model
            this.DataContext = ActiveUser;

            //assume cancel result
            Result = MessageBoxResult.Cancel;

            //register events
            OkayButton.Click += new RoutedEventHandler(OkayButton_Click);
            CancelButton.Click += new RoutedEventHandler(CancelButton_Click);
            PrivacyPolicyLink.RequestNavigate += new System.Windows.Navigation.RequestNavigateEventHandler(PrivacyPolicyLink_RequestNavigate);
        }

        void PrivacyPolicyLink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            this.Close();
        }

        void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            this.Close();
        }

        public static MessageBoxResult ShowModalDialog()
        {
            AccountWindow window = new AccountWindow();
            window.ShowDialog();
            return window.Result;
        }

        public static MessageBoxResult ShowModalDialog(OsbideUser userData)
        {
            AccountWindow window = new AccountWindow();
            window.ActiveUser = userData;
            window.ShowDialog();
            return window.Result;
        }
    }
}

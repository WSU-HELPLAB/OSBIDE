using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using OSBIDE.Library.Models;
using OSBIDE.Library.Events;
using System.Windows;
using System.Windows.Input;
using OSBIDE.Library.Commands;
using System.Runtime.Caching;
using OSBIDE.Library;
using OSBIDE.Controls.WebServices;
using OSBIDE.Library.ServiceClient;

namespace OSBIDE.Controls.ViewModels
{
    public class SubmitAssignmentViewModel : ViewModelBase
    {
        private OsbideWebServiceClient _client = null;
        private string _authToken = "";

        public SubmitAssignmentViewModel(string userName, string solutionName, string authToken)
        {
            UserName = userName;
            _authToken = authToken;
            SolutionName = solutionName;
            _client = new OsbideWebServiceClient(ServiceBindings.OsbideServiceBinding, ServiceBindings.OsbideServiceEndpoint);
            ContinueCommand = new DelegateCommand(Continue, CanIssueCommand);
            CancelCommand = new DelegateCommand(Cancel, CanIssueCommand);
            Assignments = new ObservableCollection<Assignment>();
            Courses = new ObservableCollection<Course>();

            //set up event listeners
            _client.GetCoursesForUserCompleted += _client_GetCoursesForUserCompleted;
            _client.GetAssignmentsForCourseCompleted += _client_GetAssignmentsForCourseCompleted;
            _client.SubmitAssignmentCompleted += _client_SubmitAssignmentCompleted;

            //load courses
            IsLoading = true;
            _client.GetCoursesForUserAsync(authToken);
        }

        void _client_SubmitAssignmentCompleted(object sender, SubmitAssignmentCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        //will populate the assignment dropdown
        void _client_GetAssignmentsForCourseCompleted(object sender, GetAssignmentsForCourseCompletedEventArgs e)
        {
            IsLoading = false;
            Assignments.Clear();
            try
            {
                foreach (Assignment a in e.Result)
                {
                    Assignments.Add(a);
                }
            }
            catch (Exception)
            {
            }
        }
        
        //will populate the courses dropdown based on the current user's settings
        void _client_GetCoursesForUserCompleted(object sender, GetCoursesForUserCompletedEventArgs e)
        {
            IsLoading = false;
            Courses.Clear();
            try
            {
                foreach (Course c in e.Result)
                {
                    Courses.Add(c);
                }
            }
            catch (Exception)
            {
            }
        }

        #region properties

        public event EventHandler RequestClose = delegate { };

        public ObservableCollection<Assignment> Assignments { get; set; }
        public ObservableCollection<Course> Courses { get; set; }
        public MessageBoxResult Result { get; private set; }
        public ICommand ContinueCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        private bool _isLoading;
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                _isLoading = value;
                OnPropertyChanged("IsLoading");
            }
        }

        private int _selectedAssignment = -1;
        public int SelectedAssignment
        {
            get
            {
                return _selectedAssignment;
            }
            set
            {
                _selectedAssignment = value;
                OnPropertyChanged("SelectedAssignment");
                OnPropertyChanged("HasAssignmentSelected");
            }
        }

        public bool HasAssignmentSelected
        {
            get
            {
                return _selectedAssignment > 0;
            }
        }

        private int _selectedCourse = -1;
        public int SelectedCourse
        {
            get
            {
                return _selectedCourse;
            }
            set
            {
                _selectedCourse = value;

                //load assignments
                IsLoading = true;
                _client.GetAssignmentsForCourseAsync(_selectedCourse, _authToken);

                OnPropertyChanged("SelectedCourse");
                OnPropertyChanged("HasCourseSelected");
            }
        }

        public bool HasCourseSelected
        {
            get
            {
                return _selectedCourse > 0;
            }
        }

        public string UserName
        {
            private set;
            get;
        }

        private string _lastSubmitted = "";
        public string LastSubmitted
        {
            get
            {
                return _lastSubmitted;
            }
            set
            {
                _lastSubmitted = value;
                OnPropertyChanged("LastSubmitted");
            }
        }

        private string _solutionName = "";
        public string SolutionName
        {
            get
            {
                return _solutionName;
            }
            set
            {
                _solutionName = value;
                OnPropertyChanged("SolutionName");
            }
        }

        #endregion

        private void Continue(object param)
        {
            Result = MessageBoxResult.OK;
            RequestClose(this, EventArgs.Empty);
        }

        private void Cancel(object param)
        {
            Result = MessageBoxResult.Cancel;
            RequestClose(this, EventArgs.Empty);
        }

        private bool CanIssueCommand(object param)
        {
            return true;
        }
    }
}

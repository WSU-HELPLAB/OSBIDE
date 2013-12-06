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

namespace OSBIDE.Controls.ViewModels
{
    public class SubmitAssignmentViewModel : ViewModelBase
    {
        private ObjectCache _cache = new FileCache(StringConstants.LocalCacheDirectory, new LibraryBinder());
        public SubmitAssignmentViewModel(string userName, IOsbideEvent lastSubmit)
        {
            UserName = userName;
            LastSubmit = lastSubmit;
            ContinueCommand = new DelegateCommand(Continue, CanIssueCommand);
            CancelCommand = new DelegateCommand(Cancel, CanIssueCommand);
            Assignments = new ObservableCollection<string>();
        }

        #region properties

        public event EventHandler RequestClose = delegate { };

        public ObservableCollection<string> Assignments { get; set; }
        public ObservableCollection<string> Courses { get; set; }
        public MessageBoxResult Result { get; private set; }
        public ICommand ContinueCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        private string _selectedAssignment = "";
        public string SelectedAssignment
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
                return _selectedAssignment.Trim().Length > 0;
            }
        }

        private string _selectedCourse = "";
        public string SelectedCourse
        {
            get
            {
                return _selectedCourse;
            }
            set
            {
                _selectedCourse = value;
                OnPropertyChanged("SelectedCourse");
                OnPropertyChanged("HasCourseSelected");
            }
        }

        public bool HasCourseSelected
        {
            get
            {
                return _selectedCourse.Trim().Length > 0;
            }
        }

        private IOsbideEvent _submitEvent;
        public IOsbideEvent LastSubmit
        {
            get
            {
                return _submitEvent;
            }
            set
            {
                _submitEvent = value;
                LastSubmitted = LastSubmit.EventDate.ToLongDateString();
                SolutionName = LastSubmit.SolutionName;
                OnPropertyChanged("LastSubmit");
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

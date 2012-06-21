using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using OSBIDE.Library.Models;
using OSBIDE.Library.Events;
using System.Windows.Input;
using OSBIDE.Library.Commands;
using Microsoft.Win32;
using System.IO;
using Ionic.Zip;
using System.Windows.Forms;
using OSBIDE.Library;

namespace OSBIDE.Controls.ViewModels
{
    public class AssignmentSubmissionsViewModel : ViewModelBase
    {
        private string _selectedAssignment = "";
        private OsbideContext _db;
        private List<EventLog> _allSubmissions = new List<EventLog>();
        private string _errorMessage = "";

        public ObservableCollection<string> AvailableAssignments { get; set; }
        public ObservableCollection<SubmissionEntryViewModel> SubmissionEntries { get; set; }
        public ICommand DownloadCommand { get; set; }
        public string SelectedAssignment
        {
            get
            {
                return _selectedAssignment;
            }
            set
            {
                _selectedAssignment = value;
                GetSubmissionEntries();
                OnPropertyChanged("SelectedAssignment");
            }
        }

        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            private set
            {
                _errorMessage = value;
                OnPropertyChanged("ErrorMessage");
            }
        }

        public AssignmentSubmissionsViewModel()
        {
        }

        public AssignmentSubmissionsViewModel(OsbideContext db)
        {
            DownloadCommand = new DelegateCommand(Download, CanIssueCommand);
            AvailableAssignments = new ObservableCollection<string>();
            SubmissionEntries = new ObservableCollection<SubmissionEntryViewModel>();
            
            _db = db;
            var names = (from submit in db.SubmitEvents
                         select submit.AssignmentName).Distinct().ToList();
            foreach (string name in names)
            {
                AvailableAssignments.Add(name);
            }
        }

        void osbideState_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HasSqlServerError")
            {
                ErrorMessage = "Error retrieving record...";
            }
            else if (e.PropertyName == "HasWebServiceError")
            {
                ErrorMessage = "Error contacting server...";
            }
        }

        private void GetSubmissionEntries()
        {
            var entries = from submit in _db.SubmitEvents
                          where submit.AssignmentName == SelectedAssignment
                          //&& submit.EventLog.SenderId == OsbideUser.CurrentUser.Id
                          orderby submit.EventDate ascending
                          select submit;
            Dictionary<string, SubmitEvent> events = new Dictionary<string, SubmitEvent>();
            foreach (SubmitEvent evt in entries)
            {
                events[evt.EventLog.Sender.FullName] = evt;
            }

            SubmissionEntries.Clear();
            foreach (string key in events.Keys.OrderBy(k => k.ToString()))
            {
                SubmitEvent evt = events[key];
                SubmissionEntryViewModel vm = new SubmissionEntryViewModel();
                vm.Submission = evt;
                SubmissionEntries.Add(vm);
            }
        }

        private void Download(object param)
        {
            FolderBrowserDialog save = new FolderBrowserDialog();
            DialogResult result = save.ShowDialog();
            if (result == DialogResult.OK)
            {
                string directory = save.SelectedPath;
                foreach (SubmissionEntryViewModel vm in SubmissionEntries)
                {
                    string unpackDir = Path.Combine(directory, vm.SubmissionLog.Sender.FullName);
                    using(MemoryStream zipStream = new MemoryStream())
                    {
                        zipStream.Write(vm.Submission.SolutionData, 0, vm.Submission.SolutionData.Length);
                        zipStream.Position = 0;
                        using (ZipFile zip = ZipFile.Read(zipStream))
                        {
                            foreach (ZipEntry entry in zip)
                            {
                                try
                                {
                                    entry.Extract(unpackDir, ExtractExistingFileAction.OverwriteSilently);
                                }
                                catch (BadReadException)
                                {
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }

                    //notify OSBIDE that someone has downloaded a submission
                    EventGenerator generator = EventGenerator.GetInstance();
                    generator.NotifySolutionDownloaded(OsbideUser.CurrentUser, vm.Submission);
                }
            }
        }


        private bool CanIssueCommand(object param)
        {
            return true;
        }
    }
}

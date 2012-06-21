using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSBIDE.Controls.ViewModels
{
    public class OsbideStatusViewModel : ViewModelBase
    {

        private AssignmentSubmissionsViewModel _submissionViewModel;
        public AssignmentSubmissionsViewModel SubmissionViewModel
        {
            get
            {
                return _submissionViewModel;
            }
            set
            {
                _submissionViewModel = value;
                OnPropertyChanged("SubmissionViewModel");
            }
        }

        private TransmissionStatusViewModel _statusViewModel;
        public TransmissionStatusViewModel StatusViewModel
        {
            get
            {
                return _statusViewModel;
            }
            set
            {
                _statusViewModel = value;
                OnPropertyChanged("StatusViewModel");
            }
        }
    }
}

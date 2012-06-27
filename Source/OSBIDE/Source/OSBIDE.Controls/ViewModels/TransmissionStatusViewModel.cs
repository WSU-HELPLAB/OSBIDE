using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.Library.ServiceClient;
using System.Windows.Threading;
using System.Windows.Input;
using OSBIDE.Library.Commands;

namespace OSBIDE.Controls.ViewModels
{
    public class TransmissionStatusViewModel : ViewModelBase
    {
        private ServiceClient _serviceClient;
        private DispatcherTimer _initTimer = new DispatcherTimer();
        private DispatcherTimer _updateTimer = new DispatcherTimer();

        #region view properties

        public ICommand ActivateSendCommand { get; set; }
        public ICommand ActivateReceiveCommand { get; set; }

        private string _sendStatus = "";
        public string SendStatus
        {
            get
            {
                return _sendStatus;
            }
            set
            {
                _sendStatus = value;
                OnPropertyChanged("SendStatus");
            }
        }

        private DateTime _lastSendDate = DateTime.MinValue;
        public DateTime LastSendDate
        {
            get
            {
                return _lastSendDate;
            }
            set
            {
                _lastSendDate = value;
                OnPropertyChanged("LastSendDate");
            }
        }

        private int _sendProgressMaxValue = 0;
        public int SendProgressMaxValue
        {
            get
            {
                return _sendProgressMaxValue;
            }
            set
            {
                _sendProgressMaxValue = value;
                OnPropertyChanged("SendProgressMaxValue");
            }
        }

        private int _sendProgressValue = 0;
        public int SendProgressValue
        {
            get
            {
                return _sendProgressValue;
            }
            set
            {
                _sendProgressValue = value;
                OnPropertyChanged("SendProgressValue");
            }
        }

        private string _receiveStatus = "";
        public string ReceiveStatus
        {
            get
            {
                return _receiveStatus;
            }
            set
            {
                _receiveStatus = value;
                OnPropertyChanged("ReceiveStatus");
            }
        }

        private DateTime _lastReceiveDate = DateTime.MinValue;
        public DateTime LastReceiveDate
        {
            get
            {
                return _lastReceiveDate;
            }
            set
            {
                _lastReceiveDate = value;
                OnPropertyChanged("LastReceiveDate");
            }
        }

        private bool _isNotSending = false;
        public bool IsNotSending
        {
            get
            {
                return _isNotSending;
            }
            set
            {
                _isNotSending = value;
                OnPropertyChanged("IsNotSending");
            }
        }

        private bool _isNotReceiving = false;
        public bool IsNotReceiving
        {
            get
            {
                return _isNotReceiving;
            }
            set
            {
                _isNotReceiving = value;
                OnPropertyChanged("IsNotReceiving");
            }
        }

        #endregion

        public TransmissionStatusViewModel()
        {
            _serviceClient = ServiceClient.GetInstance();
            
            _initTimer.Interval = new TimeSpan(0, 0, 0, 5);
            _initTimer.Tick += new EventHandler(initTimer_Tick);

            _updateTimer.Interval = new TimeSpan(0, 1, 0);
            _updateTimer.Tick += new EventHandler(updateTimer_Tick);
            _updateTimer.Start();

            ActivateReceiveCommand = new DelegateCommand(ActivateReceive, CanIssueCommand);
            ActivateSendCommand = new DelegateCommand(ActivateSend, CanIssueCommand);

            if (_serviceClient == null)
            {
                //wait for our service client to not be null
                _initTimer.Start();
            }
            else
            {
                ServiceClientInit();
            }
        }

        void updateTimer_Tick(object sender, EventArgs e)
        {
            UpdateTransmissionStatus();
        }

        private void ActivateSend(object param)
        {
            _serviceClient.TurnOnSending();
        }

        private void ActivateReceive(object param)
        {
            _serviceClient.TurnOnReceiving();
        }

        private bool CanIssueCommand(object param)
        {
            return true;
        }

        private void UpdateTransmissionStatus()
        {
            IsNotSending = !(_serviceClient.IsSendingData);
            if (IsNotSending)
            {
                SendStatus = "Not sending";
            }
            else
            {
                SendStatus = "Sending...";
            }

            IsNotReceiving = !(_serviceClient.IsReceivingData);
            if (IsNotReceiving)
            {
                ReceiveStatus = "Not receiving";
            }
            else
            {
                ReceiveStatus = "Receiving...";
            }
            LastReceiveDate = _serviceClient.SendStatus.LastTransmissionTime;
            SendProgressMaxValue = _serviceClient.SendStatus.NumberOfTransmissions;
            SendProgressValue = _serviceClient.SendStatus.CompletedTransmissions;
            LastSendDate = _serviceClient.SendStatus.LastTransmissionTime;
        }

        private void ServiceClientInit()
        {
            if (_serviceClient == null)
            {
                _initTimer.Start();
                return;
            }

            UpdateTransmissionStatus();
            _serviceClient.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_serviceClient_PropertyChanged);
            _serviceClient.SendStatus.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(SendStatus_PropertyChanged);
            _serviceClient.ReceiveStatus.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ReceiveStatus_PropertyChanged);
        }

        void _serviceClient_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateTransmissionStatus();
        }

        void ReceiveStatus_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateTransmissionStatus();
        }

        void SendStatus_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateTransmissionStatus();
        }

        private void initTimer_Tick(object sender, EventArgs e)
        {
            _serviceClient = ServiceClient.GetInstance();
            if (_serviceClient != null)
            {
                _initTimer.Stop();
                ServiceClientInit();
            }
        }
    }
}

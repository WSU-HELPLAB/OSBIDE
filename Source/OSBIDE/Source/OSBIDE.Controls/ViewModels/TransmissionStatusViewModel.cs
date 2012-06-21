using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.Library.ServiceClient;
using System.Windows.Threading;

namespace OSBIDE.Controls.ViewModels
{
    public class TransmissionStatusViewModel : ViewModelBase
    {
        private ServiceClient _serviceClient;
        private DispatcherTimer timer = new DispatcherTimer();

        #region view properties

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
                OnPropertyChanged("IsNotSending");
            }
        }

        #endregion

        public TransmissionStatusViewModel()
        {
            _serviceClient = ServiceClient.GetInstance();
            timer.Interval = new TimeSpan(0, 0, 0, 5);
            timer.Tick += new EventHandler(timer_Tick);

            if (_serviceClient == null)
            {
                //wait for our service client to not be null
                timer.Start();
            }
            else
            {
                ServiceClientInit();
            }
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
        }

        private void ServiceClientInit()
        {
            if (_serviceClient == null)
            {
                timer.Start();
                return;
            }

            UpdateTransmissionStatus();
            _serviceClient.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_serviceClient_PropertyChanged);
            _serviceClient.SendStatus.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(SendStatus_PropertyChanged);
            _serviceClient.ReceiveStatus.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ReceiveStatus_PropertyChanged);
        }

        void _serviceClient_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSendingData")
            {
                
            }
            else if (e.PropertyName == "IsReceivingData")
            {
                
            }
        }

        void ReceiveStatus_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsActive")
            {
            }
            else if (e.PropertyName == "CurrentTransmission")
            {
            }
            else if (e.PropertyName == "LastTransmission")
            {
            }
            else if (e.PropertyName == "NumberOfTransmissions")
            {
            }
            else if (e.PropertyName == "CompletedTransmissions")
            {
            }
            else if (e.PropertyName == "LastTransmissionTime")
            {
                LastReceiveDate = _serviceClient.SendStatus.LastTransmissionTime;
            }
        }

        void SendStatus_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsActive")
            {
            }
            else if (e.PropertyName == "CurrentTransmission")
            {
            }
            else if (e.PropertyName == "LastTransmission")
            {
            }
            else if (e.PropertyName == "NumberOfTransmissions")
            {
                SendProgressMaxValue = _serviceClient.SendStatus.NumberOfTransmissions;
            }
            else if (e.PropertyName == "CompletedTransmissions")
            {
                SendProgressValue = _serviceClient.SendStatus.CompletedTransmissions;
            }
            else if (e.PropertyName == "LastTransmissionTime")
            {
                LastSendDate = _serviceClient.SendStatus.LastTransmissionTime;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            _serviceClient = ServiceClient.GetInstance();
            if (_serviceClient != null)
            {
                timer.Stop();
                ServiceClientInit();
            }
        }
    }
}

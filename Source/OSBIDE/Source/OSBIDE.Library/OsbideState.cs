using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace OSBIDE.Library
{
    /// <summary>
    /// Contains a list of global states that may be of interest to a variety of
    /// OSBIDE components
    /// </summary>
    public class ServiceClientState : INotifyPropertyChanged
    {
        #region instance variables
        private static ServiceClientState _instance = null;
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private bool _isPerformingWebPushes = true;
        private bool _isPerformingWebGets = true;
        private DateTime _lastWebPull = DateTime.Now;
        private DateTime _lastWebPush = DateTime.Now;
        #endregion

        #region properties

        /// <summary>
        /// Whether or not OSBIDE is actively attempting to get information from
        /// the server.
        /// </summary>
        public bool IsPerformingWebGets
        {
            get
            {
                lock (this)
                {
                    return _isPerformingWebGets;
                }
            }
            set
            {
                lock (this)
                {
                    _isPerformingWebGets = value;
                }
                OnPropertyChanged("IsPerformingWebGets");
            }
        }

        /// <summary>
        /// Whether or not OSBIDE is actively attempting to send information to the
        /// server.
        /// </summary>
        public bool IsPerformingWebPushes
        {
            get
            {
                lock (this)
                {
                    return _isPerformingWebPushes;
                }
            }
            set
            {
                lock (this)
                {
                    _isPerformingWebPushes = value;
                }
                OnPropertyChanged("IsPerformingWebPushes");
            }
        }

        /// <summary>
        /// The last time that OSBIDE successfully received items from the server
        /// </summary>
        public DateTime LastWebPull
        {
            get
            {
                lock (this)
                {
                    return _lastWebPull;
                }
            }
            set
            {
                lock (this)
                {
                    _lastWebPull = value;
                }
                OnPropertyChanged("LastWebPull");
            }
        }

        /// <summary>
        /// The last time that OSBIDE successfully sent items to the server
        /// </summary>
        public DateTime LastWebPush
        {
            get
            {
                lock (this)
                {
                    return _lastWebPush;
                }
            }
            set
            {
                lock (this)
                {
                    _lastWebPush = value;
                }
                OnPropertyChanged("LastWebPush");
            }
        }

        /// <summary>
        /// Singleton pattern implementation, C# style
        /// </summary>
        public static ServiceClientState Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServiceClientState();
                }
                return _instance;
            }
        }
        #endregion

        private ServiceClientState()
        {
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

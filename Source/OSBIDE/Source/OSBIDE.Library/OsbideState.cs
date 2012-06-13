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
    public class OsbideState : INotifyPropertyChanged
    {
        #region instance variables
        private static OsbideState _instance = null;
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private bool _hasSqlServerError = false;
        private bool _hasWebServiceError = false;
        #endregion

        #region properties

        /// <summary>
        /// Gets or sets whether or not the system has encountered a web service error
        /// </summary>
        public bool HasWebServiceError
        {
            get
            {
                return _hasWebServiceError;
            }
            set
            {
                _hasWebServiceError = value;
                OnPropertyChanged("HasWebServiceError");
            }
        }

        /// <summary>
        /// Gets or sets whether or not the system has encountered a SQL Server CE Error
        /// </summary>
        public bool HasSqlServerError
        {
            get
            {
                return _hasSqlServerError;
            }
            set
            {
                _hasSqlServerError = value;
                OnPropertyChanged("HasSqlServerError");
            }
        }

        /// <summary>
        /// Singleton pattern implementation, C# style
        /// </summary>
        public static OsbideState Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new OsbideState();
                }
                return _instance;
            }
        }
        #endregion

        private OsbideState()
        {
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

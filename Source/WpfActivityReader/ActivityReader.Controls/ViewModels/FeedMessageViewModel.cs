using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using ActivityReader.Library;
using OSBIDE.Library.Events;

namespace ActivityReader.Controls.ViewModels
{
    public class FeedMessageViewModel : ViewModelBase
    {
        public IOsbideEvent BaseEvent { get; set; }

        private ImageSource _messageImage;
        public ImageSource MessageImage
        {
            get
            {
                return _messageImage;
            }
            set
            {
                _messageImage = value;
                OnPropertyChanged("MessageImage");
            }
        }

        private string _messageAuthor = "";
        public string MessageAuthor
        {
            get
            {
                return _messageAuthor;
            }
            set
            {
                _messageAuthor = value;
                OnPropertyChanged("MessageAuthor");
            }
        }

        private string _messageDate = "";
        public string MessageDate
        {
            get
            {
                return _messageDate;
            }
            set
            {
                _messageDate = value;
                OnPropertyChanged("MessageDate");
            }
        }

        private string _messageContent = "";
        public string MessageContent
        {
            get
            {
                return _messageContent;
            }
            set
            {
                OnPropertyChanged("MessageContent");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using ActivityReader.Library;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using OSBIDE.Library;

namespace ActivityReader.Controls.ViewModels
{
    public class FeedMessageViewModel : ViewModelBase
    {
        private IOsbideEvent _baseEvent = null;
        public IOsbideEvent BaseEvent
        {
            get
            {
                return _baseEvent;
            }
            private set
            {
                _baseEvent = value;
                MessageDate = string.Format("{0} at {1}",
                    BaseEvent.EventDate.ToString("MM/dd/yy"),
                    BaseEvent.EventDate.ToString("hh:mm:ss tt")
                    );
                if (BaseEvent is BuildEvent)
                {
                    BuildEvent buildEvent = (BuildEvent)BaseEvent;
                    MessageContent = string.Format("{0} attempted to build {1}.", Log.Sender.FullName, BaseEvent.SolutionName);
                    if (buildEvent.ErrorItems.Count > 0)
                    {
                        MessageContent += string.Format("They received {0} errors.", buildEvent.ErrorItems.Count);
                    }
                }
                else if (BaseEvent is DebugEvent)
                {
                    DebugEvent debug = (DebugEvent)BaseEvent;
                    StringBuilder builder = new StringBuilder();
                    builder.AppendFormat("{0} is currently debugging", Log.Sender.FullName);
                    MessageContent = builder.ToString();
                }
                else if (BaseEvent is EditorActivityEvent)
                {
                    EditorActivityEvent activity = (EditorActivityEvent)BaseEvent;
                    StringBuilder builder = new StringBuilder();
                    builder.AppendFormat("{0} is currently working on {1}.", Log.Sender.FullName, activity.SolutionName);
                    MessageContent = builder.ToString();
                }
                else if (BaseEvent is ExceptionEvent)
                {
                }
                else if (BaseEvent is SaveEvent)
                {
                    SaveEvent save = (SaveEvent)BaseEvent;
                    StringBuilder builder = new StringBuilder();
                    builder.AppendFormat("{0} just saved {1}.", Log.Sender.FullName, save.Document.FileName);
                    MessageContent = builder.ToString();
                }
                else if (BaseEvent is SubmitEvent)
                {
                    SubmitEvent submit = (SubmitEvent)BaseEvent;
                    StringBuilder builder = new StringBuilder();
                    builder.AppendFormat("{0} just submitted {1}.", Log.Sender.FullName, submit.AssignmentName);
                    MessageContent = builder.ToString();
                }
                else if (BaseEvent is SolutionDownloadEvent)
                {
                    SolutionDownloadEvent download = (SolutionDownloadEvent)BaseEvent;
                    StringBuilder builder = new StringBuilder();
                    builder.AppendFormat("{0} downloaded {1}'s submission of {2}.", 
                        Log.Sender.FullName, 
                        download.AuthorId,
                        download.AssignmentName
                        );
                    MessageContent = builder.ToString();
                }
            }
        }

        private EventLog _log = null;
        public EventLog Log
        {
            get
            {
                return _log;
            }
            set
            {
                _log = value;
                BaseEvent = (IOsbideEvent)EventFactory.FromZippedBinary(Log.Data, new OsbideDeserializationBinder());
                MessageAuthor = Log.Sender.FullName;
            }
        }

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
                _messageContent = value;
                OnPropertyChanged("MessageContent");
            }
        }
    }
}

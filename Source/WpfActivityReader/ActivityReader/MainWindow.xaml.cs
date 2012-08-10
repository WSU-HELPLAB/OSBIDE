using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ActivityReader.Controls.Views;
using ActivityReader.Controls.ViewModels;
using OSBIDE.Library.Models;
using System.Data.SqlServerCe;
using ActivityReader.Library;
using OSBIDE.Library.Events;
using OSBIDE.Library;

namespace ActivityReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OsbideContext db;
        public MainWindow()
        {
            InitializeComponent();
            SqlCeConnection conn = new SqlCeConnection(ActivityReader.Library.StringConstants.SqlCeConnectionString);
            db = new OsbideContext(conn, true);
            ActivityFeedViewModel vm = new ActivityFeedViewModel();
            
        }

        private void OpenDownloadWindow(object sender, RoutedEventArgs e)
        {
            EventDownloadViewModel vm = new EventDownloadViewModel();
            EventDownloadView view = new EventDownloadView(vm);
            view.ShowDialog();
        }

        private void ProcessLogs(object sender, RoutedEventArgs e)
        {
            int counter = 0;
            List<EventLog> logs = db.EventLogs.OrderBy(l => l.Id).Skip(counter).Take(100).ToList();
            foreach (EventLog log in logs)
            {
                //turn into a factory
                if (log.LogType.CompareTo(BuildEvent.Name) == 0)
                {
                    BuildEvent build = (BuildEvent)EventFactory.FromZippedBinary(log.Data, new OsbideDeserializationBinder());
                    build.EventLogId = log.Id;
                    db.BuildEvents.Add(build);
                    db.SaveChanges();
                }
                else if (log.LogType.CompareTo(CutCopyPasteEvent.Name) == 0)
                {
                    CutCopyPasteEvent evt = (CutCopyPasteEvent)EventFactory.FromZippedBinary(log.Data, new OsbideDeserializationBinder());
                    evt.EventLogId = log.Id;
                    db.CutCopyPasteEvents.Add(evt);
                    db.SaveChanges();
                }
                else if (log.LogType.CompareTo(DebugEvent.Name) == 0)
                {
                    DebugEvent evt = (DebugEvent)EventFactory.FromZippedBinary(log.Data, new OsbideDeserializationBinder());
                    evt.EventLogId = log.Id;
                    db.DebugEvents.Add(evt);
                    db.SaveChanges();
                }
                else if (log.LogType.CompareTo(EditorActivityEvent.Name) == 0)
                {
                    EditorActivityEvent evt = (EditorActivityEvent)EventFactory.FromZippedBinary(log.Data, new OsbideDeserializationBinder());
                    evt.EventLogId = log.Id;
                    db.EditorActivityEvents.Add(evt);
                    db.SaveChanges();
                }
                else if (log.LogType.CompareTo(ExceptionEvent.Name) == 0)
                {
                    ExceptionEvent evt = (ExceptionEvent)EventFactory.FromZippedBinary(log.Data, new OsbideDeserializationBinder());
                    evt.EventLogId = log.Id;
                    db.ExceptionEvents.Add(evt);
                    db.SaveChanges();
                }
                else if (log.LogType.CompareTo(SaveEvent.Name) == 0)
                {
                    SaveEvent evt = (SaveEvent)EventFactory.FromZippedBinary(log.Data, new OsbideDeserializationBinder());
                    evt.EventLogId = log.Id;
                    db.SaveEvents.Add(evt);
                    db.SaveChanges();
                }
                else if (log.LogType.CompareTo(SubmitEvent.Name) == 0)
                {
                    SubmitEvent evt = (SubmitEvent)EventFactory.FromZippedBinary(log.Data, new OsbideDeserializationBinder());
                    evt.EventLogId = log.Id;
                    db.SubmitEvents.Add(evt);
                    db.SaveChanges();
                }
                else if (log.LogType.CompareTo(SolutionDownloadEvent.Name) == 0)
                {
                    /*
                    SolutionDownloadEvent evt = (SolutionDownloadEvent)EventFactory.FromZippedBinary(log.Data, new OsbideDeserializationBinder());
                    evt.EventLogId = log.Id;
                    db.SolutionDownloadEvents.Add(evt);
                    db.SaveChanges();
                     * */
                }
            }
        }
    }
}

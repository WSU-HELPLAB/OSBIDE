using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.VSPackage.WebServices;
using OSBIDE.Library.Models;
using OSBIDE.Library.Events;
using OSBIDE.Library;
using System.Data.SqlServerCe;
using System.Threading;

namespace OSBIDE.VSPackage
{
    public class ServiceClient
    {
        private OsbideWebServiceClient webServiceClient = null;
        private OsbideContext localDb;
        private OsbideUser currentUser;
        private EventHandlerBase events;
        private List<EventLog> pendingLogs = new List<EventLog>();
        private ILogger logger;
        private bool allowLogServiceCalls;
        private Thread localThread;
        private Thread serverThread;

        public ServiceClient(EventHandlerBase dteEventHandler, OsbideUser user, ILogger logger)
        {
            events = dteEventHandler;
            currentUser = user;
            this.logger = logger;

            //no point in continuing if we don't have SQL Server CE set up
            if (!OsbideContext.HasSqlServerCE)
            {
                logger.WriteToLog("ServiceClient did not detect SQL Server CE.  Will not log local events.");
                return;
            }
            
            webServiceClient = new OsbideWebServiceClient(ServiceBindings.OsbideServiceBinding, ServiceBindings.OsbideServiceEndpoint);
            SqlCeConnection conn = new SqlCeConnection(StringConstants.LocalDataConnectionString);
            localDb = new OsbideContext(conn, true);
            
            //AC: This scheme is a little goofy, but I can't think of a better way to do it right now.  The overall
            //problem is getting events generated on the local machine to the server.  The most obvious solution is
            //to just send off events as their created.  However, this will fail if the user is offline or the server
            //is unavailable.  As such, we need to save the data locally until we're sure that the data has been
            //received successfully by the server.  

            //Originally, I had OSBIDE save the logs to a local DB and then periodically send them to the server.
            //The problem with this approach was that Visual Studio crashes when I attempt to do a read and write
            //to the local db at the same time.  This made me think about using threads with locks, which is how
            //I came to the present solution:

            //1: Once an event is received, save it to an in-memory list (lock for mutual exclusion)
            //2: Every once in a while, attempt to save the list (lock) to the local db (lock)
            //3: On a period longer than the one defined in step #2, attempt to send the local db (lock)
            //   to the server.  Once complete, delete the records in the local db.

            //listen for new events
            events.EventCreated += new EventHandler<EventCreatedArgs>(OsbideEventCreated);

            //start the threads that will manage the local db & remote server synchronization 
            localThread = new Thread(new ThreadStart(this.AddToLocalDb));
            serverThread = new Thread(new ThreadStart(this.SendLogsToServer));
            localThread.Start();
            serverThread.Start();
        }

        ~ServiceClient()
        {
            localThread.Abort();
            serverThread.Abort();
        }

        private void AddToLocalDb()
        {
            while (true)
            {
                //run every 5 seconds
                Thread.Sleep(5000);
                EventLog[] pending;
                lock (pendingLogs)
                {
                    pending = pendingLogs.ToArray();
                    pendingLogs.Clear();
                }
                lock (localDb)
                {
                    foreach (EventLog log in pending)
                    {
                        localDb.EventLogs.Add(log);
                        localDb.SaveChanges();
                        logger.WriteToLog(string.Format("Event of type {0} created and saved to DB", log.LogType));
                    }
                }
            }
        }

        private void SendLogsToServer()
        {
            while (true)
            {
                //run every 60 seconds
                Thread.Sleep(60000);
                lock (localDb)
                {
                    //find all logs that haven't been handled (submitted)
                    List<EventLog> logs = localDb.EventLogs.Where(model => model.Handled == false).ToList();
                    List<int> savedLogs = new List<int>(logs.Count);

                    //send all unsubmitted logs to the server
                    foreach (EventLog log in logs)
                    {
                        try
                        {
                            //AC: EF attaches a bunch of crap to POCO objects for change tracking.  Said additions
                            //ruin WCF transfers.  There are supposidly fixes (see http://msdn.microsoft.com/en-us/library/dd456853.aspx)
                            //but for now, I'm just being lazy and using copy constructors to convert back to 
                            //standard objects.
                            EventLog cleanLog = new EventLog(log); //who doesn't like a clean log? :)

                            //reset the log's sending user just to be safe
                            cleanLog.SenderId = 0;
                            cleanLog.Sender = currentUser;
                            logger.WriteToLog(string.Format("Sending log with ID {0} to the server", cleanLog.Id));

                            //the number that comes back from the web service is the log's local ID number.  Save
                            //for later when we clean up our local db.
                            int result = webServiceClient.SubmitLog(cleanLog);
                            savedLogs.Add(result);
                        }
                        catch (Exception ex)
                        {
                            logger.WriteToLog(string.Format("SaveLogs error: {0}", ex.Message));
                        }
                    }

                    //clear our successfully saved logs
                    logs = null;
                    foreach (int logId in savedLogs)
                    {
                        EventLog log = localDb.EventLogs.Find(logId);
                        if (log != null)
                        {
                            logger.WriteToLog(string.Format("Removing log ID {0} from local DB", log.Id));
                            localDb.Entry(log).State = System.Data.EntityState.Deleted;
                            localDb.SaveChanges();
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Called whenever OSBIDE detects an event change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OsbideEventCreated(object sender, EventCreatedArgs e)
        {
            //add the log to the pending list
            EventLog eventLog = new EventLog(e.OsbideEvent, currentUser);
            lock(pendingLogs)
            {
                pendingLogs.Add(eventLog);
            }
        }
    }
}

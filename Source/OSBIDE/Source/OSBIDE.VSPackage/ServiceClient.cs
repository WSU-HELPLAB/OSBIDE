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

            //AC: "events" ends up being null during unit testing.  Otherwise, it should never happen.
            if (events != null)
            {
                events.EventCreated += new EventHandler<EventCreatedArgs>(OsbideEventCreated);
            }
        }

        private void SendLogToServer(object data)
        {
            //only accept eventlog data
            if (!(data is EventLog))
            {
                return;
            }

            //cast generic data to what we actually need
            EventLog newLog = data as EventLog;

            //find all logs that haven't been handled (submitted)
            List<EventLog> logsToBeSaved = null;
            lock (localDb)
            {
                //add the new log to the local db
                localDb.EventLogs.Add(newLog);
                localDb.SaveChanges();

                //then, get all pending logs
                logsToBeSaved = localDb.EventLogs.Where(model => model.Handled == false).ToList();
            }

            //will hold the list of saved logs
            List<int> savedLogs = new List<int>(logsToBeSaved.Count);
            
            //send logs to the server
            foreach (EventLog log in logsToBeSaved)
            {
                //AC: EF attaches a bunch of crap to POCO objects for change tracking.  Said additions
                //ruin WCF transfers.  There are supposidly fixes (see http://msdn.microsoft.com/en-us/library/dd456853.aspx)
                //but for now, I'm just being lazy and using copy constructors to convert back to 
                //standard objects.
                EventLog cleanLog = new EventLog(newLog); //who doesn't like a clean log? :)

                //reset the log's sending user just to be safe
                cleanLog.SenderId = 0;
                cleanLog.Sender = currentUser;
                logger.WriteToLog(string.Format("Sending log with ID {0} to the server", cleanLog.Id));

                try
                {
                    //the number that comes back from the web service is the log's local ID number.  Save
                    //for later when we clean up our local db.
                    int result = webServiceClient.SubmitLog(cleanLog);
                    savedLogs.Add(result);
                }
                catch (Exception ex)
                {
                    //Log any error that we might've received.  Most likely an issue finding the endpoint (server)
                    logger.WriteToLog(string.Format("SaveLogs error: {0}", ex.Message));
                }
            }

            //finally clear our successfully saved logs
            logsToBeSaved = null;
            lock (localDb)
            {
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


        /// <summary>
        /// Called whenever OSBIDE detects an event change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OsbideEventCreated(object sender, EventCreatedArgs e)
        {
            //create a new event log...
            EventLog eventLog = new EventLog(e.OsbideEvent, currentUser);
            
            //...and send it off to the server thread for saving
            Thread serverThread = new Thread(this.SendLogToServer);
            serverThread.Start(eventLog);
        }
    }
}

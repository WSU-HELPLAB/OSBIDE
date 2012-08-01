using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using OSBIDE.Library.Models;
using System.Data.Entity;
using System.Threading;
using OSBIDE.Library.Events;

namespace OSBIDE.Web
{
    [ServiceContract(Namespace = "")]
    [SilverlightFaultBehavior]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class OsbideWebService
    {
        public OsbideContext Db { get; private set; }

        public OsbideWebService()
        {
#if DEBUG
            Db = new OsbideContext("OsbideDebugContext");
            Database.SetInitializer<OsbideContext>(new DropCreateDatabaseIfModelChanges<OsbideContext>());
#else
            Db = new OsbideContext("OsbideReleaseContext");
#endif
        }

        [OperationContract]
        public string Echo(string toEcho)
        {
            return toEcho;
        }

        [OperationContract]
        [ApplyDataContractResolver]
        public OsbideUser GetUserById(int id)
        {
            return Db.Users.Find(id);
        }

        [OperationContract]
        [ApplyDataContractResolver]
        public OsbideUser SaveUser(OsbideUser userToSave)
        {
            //reset sender id
            userToSave.Id = 0;

            //make sure that the user is valid
            if (
                    userToSave.FirstName == null 
                    || 
                    userToSave.LastName == null
                    ||
                    userToSave.InstitutionId == null
                    ||
                    userToSave.FirstName.Length == 0 
                    || 
                    userToSave.LastName.Length == 0 
                    ||
                    userToSave.InstitutionId.Length == 0
                )
            {
                return userToSave;
            }

            //try to find the user in the DB before creating a new record
            OsbideUser dbUser = (from user in Db.Users
                                 where
                                 user.FirstName.CompareTo(userToSave.FirstName) == 0
                                 &&
                                 user.LastName.CompareTo(userToSave.LastName) == 0
                                 &&
                                 user.InstitutionId.CompareTo(userToSave.InstitutionId) == 0
                                 select user).FirstOrDefault();
            if (dbUser != null)
            {
                userToSave.Id = dbUser.Id;
            }
            else
            {
                Db.Users.Add(userToSave);
                Db.SaveChanges();
            }
            return userToSave;
        }

        [OperationContract]
        [ApplyDataContractResolver]
        public int SubmitLocalErrorLog(LocalErrorLog errorLog)
        {
            //check to see if the user exists in the database.  If not, reset sender and try again
            //(next IF statement)
            if (errorLog.SenderId != 0)
            {
                OsbideUser userCheck = Db.Users.Find(errorLog.SenderId);
                if (userCheck == null)
                {
                    errorLog.SenderId = 0;
                }
            }

            //reset the sender if necessary
            if (errorLog.SenderId == 0)
            {
                errorLog.Sender = SaveUser(errorLog.Sender);
                errorLog.SenderId = errorLog.Sender.Id;
            }
            errorLog.Sender = null;
            Db.LocalErrorLogs.Add(errorLog);
            try
            {
                Db.SaveChanges();
            }
            catch (Exception)
            {
                return (int)Enums.ServiceCode.Error;
            }
            return errorLog.Id;
        }

        [OperationContract]
        [ApplyDataContractResolver]
        public int SubmitLog(EventLog log)
        {
            //AC: kind of hackish, but event logs that we receive should already have an ID
            //attached to them from being stored in the machine's local DB.  We can use 
            //that ID to track the success/failure of asynchronous calls.
            int localId = log.Id;

            //we don't want the local id, so be sure to clear
            log.Id = 0;

            //check to see if the user exists in the database.  If not, reset sender and try again
            //(next IF statement)
            if (log.SenderId != 0)
            {
                OsbideUser userCheck = Db.Users.Find(log.SenderId);
                if (userCheck == null)
                {
                    log.SenderId = 0;
                }
            }

            //reset the sender if necessary
            if (log.SenderId == 0)
            {
                log.Sender = SaveUser(log.Sender);
                log.SenderId = log.Sender.Id;
            }
            log.Sender = null;
            Db.EventLogs.Add(log);
            try
            {
                Db.SaveChanges();
            }
            catch (Exception)
            {
                return (int)Enums.ServiceCode.Error;
            }

            //Return the ID number of the local object so that the caller knows that it's been successfully
            //saved into the main system.
            return localId;
        }

        /// <summary>
        /// Returns all events that occur after "start"
        /// </summary>
        /// <param name="start"></param>
        /// <param name="waitForContent">Whether or not the web service should wait for new content before returning</param>
        /// <returns></returns>
        [OperationContract]
        public List<EventLog> GetPastEvents(DateTime start, bool waitForContent = true)
        {
            bool foundData = false;
            int timeoutCount = 0;
            List<EventLog> logs = new List<EventLog>();
            while (foundData == false)
            {
                timeoutCount++;
                logs = (from log in Db.EventLogs
                        where log.DateReceived > start
                        orderby log.DateReceived ascending
                        select log
                        ).Take(10).ToList();
                if (logs.Count > 0 || waitForContent == false || timeoutCount > 4)
                {
                    foundData = true;
                }
                else
                {
                    //sleep 20 seconds before making another request
                    Thread.Sleep(new TimeSpan(0, 0, 10));
                }
            }

            //Remove all of the dynamic EF crap before sending over the wire.
            //Needed or else an connection exception will get thrown.
            List<EventLog> staticLogs = new List<EventLog>(logs.Count);
            foreach (EventLog log in logs)
            {
                staticLogs.Add(new EventLog(log));
            }
            return staticLogs;
        }

        [OperationContract]
        public List<OsbideUser> GetUsers(int[] osbideIds)
        {
            List<OsbideUser> users = new List<OsbideUser>();
            var query = from user in Db.Users
                        where osbideIds.Contains(user.Id)
                        select user;
            
            foreach (OsbideUser user in query)
            {
                user.InstitutionId = "withheld";
                users.Add(new OsbideUser(user));
            }
            return users;
        }

        [OperationContract]
        public string LibraryVersionNumber()
        {
            return OSBIDE.Library.StringConstants.LibraryVersion;
        }

        [OperationContract]
        public string OsbidePackageUrl()
        {
            return OSBIDE.Library.StringConstants.OsbidePackageUrl;
        }
    }
}

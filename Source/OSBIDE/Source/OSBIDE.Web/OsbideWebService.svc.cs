using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using OSBIDE.Library.Models;
using System.Data.Entity;

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
        public int SubmitLog(EventLog log)
        {
            //AC: kind of hackish, but event logs that we receive should already have an ID
            //attached to them from being stored in the machine's local DB.  We can use 
            //that ID to track the success/failure of asynchronous calls.
            int localId = log.Id;

            //we don't want the local id, so be sure to clear
            log.Id = 0;

            //also, reset the sender if necessary
            if (log.SenderId == 0)
            {
                log.Sender = SaveUser(log.Sender);
                log.SenderId = log.Sender.Id;
            }
            log.Sender = null;

            //if we've gotten this far, we're probably okay to mark the log as being
            //handled
            log.Handled = true;
            Db.EventLogs.Add(log);
            try
            {
                Db.SaveChanges();
            }
            catch (Exception ex)
            {
                return (int)Enums.ServiceCode.Error;
            }

            //Return the ID number of the local object so that the caller knows that it's been successfully
            //saved into the main system.
            return localId;
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

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using OSBIDE.Library.Models;

namespace OSBIDE.Web
{
    [ServiceContract(Namespace = "")]
    [SilverlightFaultBehavior]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class OsbideWebService
    {
        private OsbideContext Db;

        public OsbideWebService()
        {
            Db = new OsbideContext();
        }

        [OperationContract]
        public string Echo(string toEcho)
        {
            return toEcho;
        }

        [OperationContract]
        public OsbideUser SaveUser(OsbideUser userToSave)
        {
            //ignore users that already have ID numbers
            if (userToSave.Id == 0)
            {
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
                }
            }
            return userToSave;
        }

        [OperationContract]
        public int SubmitLog(EventLog log)
        {
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
            return (int)Enums.ServiceCode.Ok;
        }
    }
}

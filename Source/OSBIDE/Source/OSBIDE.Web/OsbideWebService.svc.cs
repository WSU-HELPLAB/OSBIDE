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
        public int SubmitLog(string LogType, byte[] data)
        {
            EventLog log = new EventLog()
            {
                Data = data,
                LogType = LogType,
                Handled = true
            };
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

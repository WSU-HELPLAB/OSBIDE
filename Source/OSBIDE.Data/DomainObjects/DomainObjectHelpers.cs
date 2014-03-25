using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using OSBIDE.Data.NoSQLStorage;

namespace OSBIDE.Data.DomainObjects
{
    public static class DomainObjectHelpers
    {
        public static void LogAccountRequest(ActionRequestLog log)
        {
            using (var storage = new ActionRequestLogTable())
            {
                var utc = DateTime.UtcNow;
                var entity = new ActionRequestLogEntity
                {
                    PartitionKey = log.SchoolId.ToString(CultureInfo.InvariantCulture),
                    RowKey = string.Format("{0}_{1}_{2}_{3}_{4}_{5}",
                                            log.CreatorId.ToString(CultureInfo.InstalledUICulture),
                                            utc.ToLongDateString(),
                                            utc.ToLongTimeString(),
                                            log.ControllerName,
                                            log.ActionName,
                                            log.ActionParameters),
                    ControllerName = log.ControllerName,
                    ActionParameters = log.ActionParameters,
                    ActionName = log.ActionName,
                    AccessDate = log.AccessDate,
                    IpAddress = log.IpAddress,
                    CreatorId=log.CreatorId,
                };
                storage.Insert(entity);
            }
        }

        public static void LogAccountRequest(IEnumerable<ActionRequestLog> logs)
        {
            using (var storage = new ActionRequestLogTable())
            {
                storage.Insert(logs.Select(log => new ActionRequestLogEntity
                {
                    PartitionKey = log.SchoolId.ToString(CultureInfo.InvariantCulture),
                    RowKey = string.Format("{0}_{1}", log.CreatorId.ToString(CultureInfo.InstalledUICulture), log.CreatorId.ToString(CultureInfo.InvariantCulture)),
                    ControllerName = log.ControllerName,
                    ActionParameters = log.ActionParameters,
                    ActionName = log.ActionName,
                    AccessDate = log.AccessDate,
                    IpAddress = log.IpAddress,
                    CreatorId=log.CreatorId,
                }));
            }
        }

        public static IEnumerable<ActionRequestLog> GetAccountRequest(int schoolId, int studentId)
        {
            using (var storage = new ActionRequestLogTable())
            {
                return storage.Select(schoolId.ToString(CultureInfo.InstalledUICulture), studentId.ToString(CultureInfo.InstalledUICulture))
                       .Select(log => new ActionRequestLog
                       {
                           SchoolId = Convert.ToInt32(log.PartitionKey),
                           CreatorId = Convert.ToInt32(log.RowKey.Split('_')[0]),
                           // from db context to populate the Creator = creator,
                           AccessDate = log.AccessDate,
                           ControllerName = log.ControllerName,
                           ActionName = log.ActionName,
                           ActionParameters = log.ActionParameters,
                           IpAddress = log.IpAddress,
                       });
            }
        }
    }
}

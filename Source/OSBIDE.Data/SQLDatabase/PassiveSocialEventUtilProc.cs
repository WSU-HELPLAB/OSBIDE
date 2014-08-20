using System.Collections.Generic;
using System.Linq;
using System.Text;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase.Edmx;
using OSBIDE.Library.Models;

namespace OSBIDE.Data.SQLDatabase
{
    public class PassiveSocialEventUtilProc
    {
        private const int BATCH_SIZE = 100;

        public static string GetSQL(int? schoolId)
        {
            var tsql = new StringBuilder();
            IEnumerable<PassiveSocialEvent> events;

            if (schoolId.HasValue)
            {
                events = DomainObjectHelpers.GetPassiveSocialActivities(schoolId.Value);
            }
            else
            {
                events = GetBatch(GetProcessedCounts(), GetActionRequestCounts());
            }

            foreach (var e in events)
            {
                tsql.AppendFormat(SQLTemplatePassiveSocialEvents.Insert, e.EventLogId, e.UserId, e.EventDate);
            }

            return tsql.ToString();
        }

        public static bool ProcessAzureTableStorage(int schoolId)
        {
            var passiveSocialEvents = DomainObjectHelpers.GetPassiveSocialActivities(schoolId).ToList();
            var totalCounts = passiveSocialEvents.Count;

            var batches = totalCounts / BATCH_SIZE;
            for (var b = 0; b < batches + 1; b++)
            {
                var tsql = new StringBuilder();

                var firstRow = b * BATCH_SIZE;
                var lastRow = (b + 1) * BATCH_SIZE > totalCounts ? totalCounts : (b + 1) * BATCH_SIZE;

                for (var idx = firstRow; idx < lastRow; idx++)
                {
                    tsql.AppendFormat(SQLTemplatePassiveSocialEvents.Insert, passiveSocialEvents[idx].EventLogId, passiveSocialEvents[idx].UserId, passiveSocialEvents[idx].EventDate);
                }

                DynamicSQLExecutor.Execute(tsql.ToString());
            }

            return true;
        }

        public static bool ProcessSQLTableStorage()
        {
            var processedCounts = GetProcessedCounts();
            var totalCounts = GetActionRequestCounts();

            var batches = (totalCounts - processedCounts) / BATCH_SIZE;
            for (var b = 0; b < batches + 1; b++)
            {
                var tsql = new StringBuilder();

                var firstRow = b * BATCH_SIZE;
                var lastRow = (b + 1) * BATCH_SIZE > totalCounts ? totalCounts : (b + 1) * BATCH_SIZE;

                var passiveSocialEvents = GetBatch(firstRow + processedCounts, lastRow - firstRow);
                foreach (var e in passiveSocialEvents)
                {
                    tsql.AppendFormat(SQLTemplatePassiveSocialEvents.Insert, e.EventLogId, e.UserId, e.EventDate);
                }

                DynamicSQLExecutor.Execute(tsql.ToString());
            }

            return true;
        }

        private static int GetProcessedCounts()
        {
            using (var context = new OsbideProcs())
            {
                var processedCount = context.GetPassiveSocialEventsCount().SingleOrDefault();
                return processedCount == null || !processedCount.HasValue ? 0 : processedCount.Value;
            }
        }

        private static int GetActionRequestCounts()
        {
            using (var context = new OsbideProcs())
            {
                var count = context.GetActionRequestsCount().SingleOrDefault();
                return count == null || !count.HasValue ? 0 : count.Value;
            }
        }

        private static IEnumerable<PassiveSocialEvent> GetBatch(int skip, int take)
        {
            using (var context = new OsbideProcs())
            {
                return (from e in context.GetPassiveSocialEvents(skip, take)
                        select new PassiveSocialEvent
                        {
                            EventLogId = e.EventLogId,
                            UserId = e.UserId,
                            EventDate = e.EventDate,
                        })
                        .ToList();
            }
        }
    }
}

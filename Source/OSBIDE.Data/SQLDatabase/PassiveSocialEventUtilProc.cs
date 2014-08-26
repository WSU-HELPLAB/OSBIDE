using System.Linq;
using System.Text;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase.Edmx;

namespace OSBIDE.Data.SQLDatabase
{
    public class PassiveSocialEventUtilProc
    {
        private const int BATCH_SIZE = 100;
        private const string DESTINATIONTABLE = "[PassiveSocialEvents]";

        public static bool Run(int schoolId)
        {
            var passiveSocialEvents = DomainObjectHelpers.GetPassiveSocialActivities(schoolId).ToList();
            var totalCounts = passiveSocialEvents.Count;

            using (var context = new OsbideProcs())
            {
                // storage tables not completely processed
                foreach (var table in context.GetPassiveSocialEventProcessLog())
                {
                    var processedCounts = table.ProcessedRecordCounts.HasValue ? table.ProcessedRecordCounts.Value : 0;
                    var batches = (totalCounts - processedCounts) / BATCH_SIZE;
                    for (var b = 0; b < batches + 1; b++)
                    {
                        var tsql = new StringBuilder();

                        var firstRow = processedCounts + b * BATCH_SIZE;
                        var lastRow = processedCounts + (b + 1) * BATCH_SIZE > totalCounts ? totalCounts : processedCounts + (b + 1) * BATCH_SIZE;

                        for (var idx = firstRow; idx < lastRow; idx++)
                        {
                            tsql.AppendFormat(SQLTemplatePassiveSocialEvents.Insert.Replace("DESTINATIONTABLE", DESTINATIONTABLE), passiveSocialEvents[idx].EventLogId, passiveSocialEvents[idx].UserId, passiveSocialEvents[idx].EventDate);
                        }

                        DynamicSQLExecutor.Execute(tsql.ToString());

                        context.UpdatePassiveSocialEventProcessLog(table.Id, DESTINATIONTABLE, lastRow == totalCounts, lastRow);
                    }
                }
            }

            return true;
        }
    }
}

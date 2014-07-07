using System;
using System.Collections.Generic;
using OSBIDE.Data.DomainObjects;

namespace OSBIDE.Data.SQLDatabase
{
    public partial class TimelineChartDataProc
    {
        public static List<TimelineChartData> Get(DateTime? dateFrom, DateTime? dateTo, IEnumerable<int> userIds, TimeScale timescale, int? timeout, bool grayscale)
        {
            var unspecifiedDate = new DateTime(2000, 1, 1);
            var timeoutVal = timeout.HasValue ? timeout.Value : VisualizationParams.DEFAULT_TIMEOUT;

            return GetTimelineDataMinuteScale(dateFrom, dateTo, userIds, grayscale, unspecifiedDate, timescale, timeoutVal);
        }
    }
}

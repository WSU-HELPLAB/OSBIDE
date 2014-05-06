using System;
using System.Collections.Generic;
using System.Linq;

using OSBIDE.Data.DomainObjects;

namespace OSBIDE.Data.SQLDatabase.DataAnalytics
{
    public class ErrorQuotientAnalytics
    {
        public static List<ErrorQuotientResult> GetResults(ErrorQuotientParams eparams, DateTime? dateFrom, DateTime? dateTo, List<int> users)
        {
            var results = new List<ErrorQuotientResult>();

            if (users != null && users.Count > 0)
            {
                var eventSessions = ErrorQuotientSessionDataProc.Get(dateFrom, dateTo, users);

                foreach (var u in users)
                {
                    if (!results.Any(r => r.UserId == u))
                    {
                        results.Add(new ErrorQuotientResult
                        {
                            UserId = u,
                            Score = ErrorQuotient.Calculate(eparams, eventSessions.Where(e => e.UserId == u))
                        });
                    }
                }
            }

            return results;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase.Edmx;

namespace OSBIDE.Data.SQLDatabase
{
    public class ErrorQuotientSessionDataProc
    {
        public static List<ErrorQuotientEvent> Get(DateTime? dateFrom, DateTime? dateTo, IList<int> userIds)
        {
            using (var context = new OsbideProcs())
            {
                var minDate = new DateTime(2000, 1, 1);
                var events = (from e in context.GetErrorQuotientSessionData((!dateFrom.HasValue || dateFrom.Value < minDate) ? minDate : dateFrom,
                                                                            (!dateTo.HasValue || dateTo.Value < minDate) ? DateTime.Today : dateTo,
                                                                            string.Join(",", userIds))
                              select new ErrorQuotientEvent
                                         {
                                             BuildId = e.BuildId,
                                             LogId = e.LogId,
                                             UserId = e.SenderId,
                                             EventDate = e.EventDate,
                                         }).ToList();

                var errorTypes = (from t in context.GetErrorQuotientErrorTypeData(string.Join(",", events.Select(e=>e.LogId))) select t).ToList();
                var errorDocs = (from d in context.GetErrorQuotientDocumentData(string.Join(",", events.Select(e => e.BuildId))) select d).ToList();

                foreach (var e in events)
                {
                    var et = errorTypes.Where(t => t.LogId == e.LogId).Select(t=>t.ErrorTypeId).ToList();
                    if (et.Count > 0)
                    {
                        e.ErrorTypeIds = et;
                    }

                    var ed = errorDocs.Where(d => d.BuildId == e.BuildId)
                                      .Select(d => new ErrorDocumentInfo {
                                                        DocumentId=d.DocumentId,
                                                        Line=d.Line,
                                                        Column=d.Column
                                                        }).ToList();
                    if (ed.Count > 0)
                    {
                        e.Documents = ed;
                    }
                }

                return events;
            }
        }
    }
}

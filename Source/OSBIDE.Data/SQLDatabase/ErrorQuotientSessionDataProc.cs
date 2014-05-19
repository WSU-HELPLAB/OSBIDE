using System;
using System.Collections.Generic;
using System.Linq;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase.Edmx;

namespace OSBIDE.Data.SQLDatabase
{
    public class ErrorQuotientSessionDataProc
    {
        /// <summary>
        /// build a list of qualified error quotient events with error types and documents
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public static List<ErrorQuotientEvent> Get(DateTime? dateFrom, DateTime? dateTo, IList<int> userIds)
        {
            using (var context = new OsbideProcs())
            {
                // filter qualified error quotient event for the selected users
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

                // error types for the resultant error quotient events
                var errorTypes = (from t in context.GetErrorQuotientErrorTypeData(string.Join(",", events.Select(e => e.LogId))) select t).ToList();

                // error documents for the resultant error quotient events
                var errorDocs = (from d in context.GetErrorQuotientDocumentData(string.Join(",", events.Select(e => e.BuildId))) select d).ToList();

                // associate error types and documents to error quotient events
                foreach (var e in events)
                {
                    // error types
                    var et = errorTypes.Where(t => t.LogId == e.LogId).Select(t => t.ErrorTypeId).ToList();
                    if (et.Count > 0)
                    {
                        e.ErrorTypeIds = et;
                    }

                    // error documents
                    var ed = errorDocs.Where(d => d.BuildId == e.BuildId)
                                      .Select(d => new ErrorDocumentInfo
                                      {
                                          DocumentId = d.DocumentId,
                                          Line = d.Line,
                                          Column = d.Column,
                                          FileName = d.FileName,
                                          NumberOfModified = d.NumberOfModified.HasValue ? d.NumberOfModified.Value : 0,
                                          ModifiedLines = d.ModifiedLines.Length > 0 ? d.ModifiedLines.Split(',').Select(l => Convert.ToInt32(l)).ToList() : null
                                      })
                                      .ToList();
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

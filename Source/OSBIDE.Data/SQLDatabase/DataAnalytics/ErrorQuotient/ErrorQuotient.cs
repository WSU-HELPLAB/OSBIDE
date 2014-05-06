using System.Collections.Generic;
using System.Linq;

using OSBIDE.Data.DomainObjects;

namespace OSBIDE.Data.SQLDatabase.DataAnalytics
{
    public class ErrorQuotient
    {
        public static decimal Calculate(ErrorQuotientParams eparams, IEnumerable<ErrorQuotientEvent> sessionEvents)
        {
            var orderedEvents = sessionEvents.OrderBy(e => e.EventDate).ToArray();
            var score = 0m;

            for (var i = 0; i < orderedEvents.Length - 1; i++)
            {
                var subject = orderedEvents[i];
                var nextsibling = orderedEvents[i + 1];

                // do both events end in errors?
                if (subject.ErrorTypeIds != null && nextsibling.ErrorTypeIds != null)
                {
                    // yes
                    score += 2;
                }

                // same error type?
                if (subject.ErrorTypeIds != null && nextsibling.ErrorTypeIds != null && subject.ErrorTypeIds.Intersect(nextsibling.ErrorTypeIds).Count() > 0)
                {
                    // yes
                    score += 3;

                    if (eparams.EtypeSamePenalty.HasValue)
                    {
                        score += eparams.EtypeSamePenalty.Value;
                    }
                }
                else if (eparams.EtypeDiffPenalty.HasValue)
                {
                    score += eparams.EtypeDiffPenalty.Value;
                }

                // same error location?
                if (subject.Documents != null && nextsibling.Documents != null && subject.Documents.Select(d => d.DocumentId).Intersect(nextsibling.Documents.Select(d => d.DocumentId)).Count() > 0)
                {
                    // yes
                    score += 3;

                    // same edit location?
                    if (subject.Documents.Any(d => nextsibling.Documents
                                                              .Any(nd => nd.DocumentId == d.DocumentId
                                                                      && nd.Line > d.Line - (eparams.ElineRange.HasValue ? eparams.ElineRange.Value : 1)
                                                                      && nd.Line > d.Line + (eparams.ElineRange.HasValue ? eparams.ElineRange.Value : 1)
                                                                      && nd.Column == d.Column)))
                    {
                        // yes
                        score += 1 * (eparams.TouchedMultiplier.HasValue ? eparams.TouchedMultiplier.Value : 1);

                        if (eparams.ElinePenalty.HasValue)
                        {
                            score += eparams.ElinePenalty.Value;
                        }
                    }
                }
            }

            return score > 0 ? score / (9 * orderedEvents.Length) : 0m;
        }
    }
}

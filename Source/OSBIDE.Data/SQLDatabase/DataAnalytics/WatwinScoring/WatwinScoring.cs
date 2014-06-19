using System.Collections.Generic;
using System.Linq;

using OSBIDE.Data.DomainObjects;
using System.Text.RegularExpressions;

namespace OSBIDE.Data.SQLDatabase.DataAnalytics
{
    public class WatwinScoring
    {
        public static decimal Calculate(WatwinScoringParams wparams, IEnumerable<BuildErrorEvent> sessionEvents)
        {
            var orderedEvents = sessionEvents.OrderBy(e => e.EventDate).ToArray();
            var score = 0m;
            var previous_errors = new List<string>();

            var sameErrorPenalty = wparams.SameErrorPenalty.HasValue ? wparams.SameErrorPenalty.Value : 4;
            var sameTypePenalty = wparams.SameTypePenalty.HasValue ? wparams.SameTypePenalty.Value : 4;
            var sameLinePenalty = wparams.SameLinePenalty.HasValue ? wparams.SameLinePenalty.Value : 2;
            var penalty = wparams.FastSolvePenalty.HasValue ? wparams.FastSolvePenalty.Value : 1;
            var slowSolvePenalty = wparams.SlowSolvePenalty.HasValue ? wparams.SlowSolvePenalty.Value : 25;
            var medSolvePenalty = wparams.MedSolvePenalty.HasValue ? wparams.MedSolvePenalty.Value : 15;

            string errorTypePattern = "error ([^:]+)";
            // process session events
            for (var i = 0; i < orderedEvents.Length - 1; i++)
            {
                var currentEvent = orderedEvents[i];
                var nextEvent = orderedEvents[i + 1];

                // do both events end in errors?
                if (currentEvent.ErrorTypes != null && nextEvent.ErrorTypes != null)
                {
                    // yes
                    // are the error messages exactly the same?
                    if (currentEvent.ErrorMessages.Intersect(nextEvent.ErrorMessages).Count() > 0)
                    {
                        //same message penalty
                        score += sameErrorPenalty;
                    }

                    //same error type?
                    if (currentEvent.ErrorTypes != null
                                                && nextEvent.ErrorTypes != null
                                                && currentEvent.ErrorTypes.Select(e => e.ErrorTypeId).Intersect(nextEvent.ErrorTypes.Select(e => e.ErrorTypeId)).Count() > 0)
                    {
                        //same type penalty
                        score += sameTypePenalty;
                    }
                    if (currentEvent.Documents != null
                                               && nextEvent.Documents != null
                                               && currentEvent.Documents
                                   .Select(d => d.FileName.ToLower())
                                   .Intersect(nextEvent.Documents.Select(d => d.FileName.ToLower())).Count() > 0)
                    {
                        //same line?
                        // same error location?
                        // eline-range defines what constituted an error on the “same line”
                        // a 0 would literally mean that two subsequent errors would need to be on exactly the same line
                        // while the range [-3,3] would indicate that any error within three lines
                        // in either direction would constitute being “on the same line.”
                        if (currentEvent.Documents
                                        .Any(d => nextEvent.Documents
                                                           .Any(nd => string.Compare(nd.FileName, d.FileName, true) == 0
                                                                   && nd.Line > d.Line - (wparams.ElineRange.HasValue ? wparams.ElineRange.Value : 1)
                                                                   && nd.Line < d.Line + (wparams.ElineRange.HasValue ? wparams.ElineRange.Value : 1)
                                                                   && nd.Column == d.Column)))
                        {
                            // yes
                            score += sameLinePenalty;
                        }
                    }
                }

                if (currentEvent.ErrorTypes != null && currentEvent.ErrorTypes.Count > 0)
                {
                    currentEvent.ErrorMessages.ForEach(x =>
                                                        {
                                                            if (previous_errors == null || previous_errors.Any(s => string.Compare(s, x, true) == 0))
                                                            {
                                                                var match = Regex.Match(x, errorTypePattern);
                                                                var errorTypeName = match.Groups[1].Value.TrimEnd();
                                                                var subjectErrorType = currentEvent.ErrorTypes.First(e => string.Compare(e.ErrorType, errorTypeName, true) == 0);

                                                                var mean = ErrorFixTimeStatsProc.Get().Single(y => y.ErrorTypeId == subjectErrorType.ErrorTypeId).Mean;
                                                                var stdev = ErrorFixTimeStatsProc.Get().Single(y => y.ErrorTypeId == subjectErrorType.ErrorTypeId).SDP;

                                                                if (penalty < slowSolvePenalty && subjectErrorType.FixingTime >= mean + stdev)
                                                                {
                                                                    penalty = slowSolvePenalty;
                                                                }
                                                                else if (penalty < medSolvePenalty && subjectErrorType.FixingTime >= mean - stdev)
                                                                {
                                                                    penalty = medSolvePenalty;
                                                                }
                                                            }

                                                        });

                    score += penalty;

                }

                previous_errors = currentEvent.ErrorMessages;
            }

            var max_score = slowSolvePenalty + medSolvePenalty + sameLinePenalty + sameErrorPenalty;
            return score > 0 ? decimal.Round(score / (max_score * ( orderedEvents.Length - 1 ) ), 2) : 0m;
        }
    }
}
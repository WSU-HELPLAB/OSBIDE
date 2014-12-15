using System.Collections.Generic;

namespace OSBIDE.Data.DomainObjects
{
    public enum MeasureCategory
    {
        ProgrammingEffort,
        CompilationBehavior,
        ExecutionBehavior,
        HelpBehavior,
    }

    public enum MeasureType
    {
        LinesOfCodeWritten,
        TimeSpent,
        NumberOfCompilations,
        ErrorQuotient,
        ErrorTypes,
        NumberOfExecutions,
        RuntimeErrorTypes,
        NumberOfBreakpointsSet,
        NumberOfDebuggingSessions,
        NumberOfPosts,
        NumberOfReplies,
        AverageReplyTime,
    }

    public class Measures
    {
        public Dictionary<MeasureCategory, List<MeasureType>> All = new Dictionary<MeasureCategory, List<MeasureType>>
        {
            {
                MeasureCategory.ProgrammingEffort,
                new List<MeasureType>
                {
                    MeasureType.LinesOfCodeWritten,
                    MeasureType.TimeSpent,
                }
            },
            {
                MeasureCategory.CompilationBehavior,
                new List<MeasureType>
                {
                    MeasureType.NumberOfCompilations,
                    MeasureType.ErrorQuotient,
                    MeasureType.ErrorTypes,
                }
            },
            {
                MeasureCategory.ExecutionBehavior,
                new List<MeasureType>
                {
                    MeasureType.NumberOfExecutions,
                    MeasureType.RuntimeErrorTypes,
                    MeasureType.NumberOfBreakpointsSet,
                    MeasureType.NumberOfDebuggingSessions,
                }
            },
            {
                MeasureCategory.HelpBehavior,
                new List<MeasureType>
                {
                    MeasureType.NumberOfPosts,
                    MeasureType.NumberOfReplies,
                    MeasureType.AverageReplyTime,
                }
            },
        };
    }

    public class DateTokens
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace OSBIDE.Data.DomainObjects
{
    public class Point
    {
        public string Name { get; set; }
        public double Position { get; set; }
        public DateTime TickTime { get; set; }
    }
    public class State
    {
        // the top 3 properties are used internally for calculating timeout idle state
        internal ProgrammingState ProgrammingState { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string StartTimeDisplayText
        {
            get
            {
                return string.Format("{0}/{1}/{2} {3}:{4}",
                    StartTime.Year, StartTime.Month, StartTime.Day, StartTime.Hour, StartTime.Minute);
            }
        }

        public string EndTimeDisplayText
        {
            get
            {
                return string.Format("{0}/{1}/{2} {3}:{4}",
                    EndTime.Year, EndTime.Month, EndTime.Day, EndTime.Hour, EndTime.Minute);
            }
        }
        public string TimeRange { get { return string.Format("{0} - {1}", StartTime.ToString("g"), EndTime.ToString("g")); } }
        // plotting and displaying properties
        public string Name { get; set; }
        public double StartPoint { get; set; }
        public double EndPoint { get; set; }
        public string CssClass { get; set; }
    }
    public class TimelineChartData
    {
        public int UserId { get; set; }
        public string title { get; set; }
        public List<State> measures { get; set; }
        public List<Point> markers { get; set; }
        public bool showTicks { get; set; }
        public int numTicks { get; set; }
        public int tickScale { get; set; }
    }
    public class ChartInfo
    {
        public List<TimelineChartData> DataPoints { get; set; }
        public int MaxRowCount { get; set; }
    }
}

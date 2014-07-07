using System;
using System.Collections.Generic;

namespace OSBIDE.Data.DomainObjects
{
    public class Point
    {
        public string Name { get; set; }
        public double Position { get; set; }
    }
    public class State
    {
        // the top 3 properties are used internally for calculating timeout idle state
        internal ProgrammingState ProgrammingState { get; set; }
        internal DateTime StartTime { get; set; }
        internal DateTime EndTime { get; set; }

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
    }
}

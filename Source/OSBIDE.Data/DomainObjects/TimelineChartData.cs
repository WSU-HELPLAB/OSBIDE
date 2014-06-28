using System.Collections.Generic;

namespace OSBIDE.Data.DomainObjects
{
    public class Point
    {
        public string Name { get; set; }
        public int Position { get; set; }
    }
    public class State
    {
        public string Name { get; set; }
        public int StartPoint { get; set; }
        public int EndPoint { get; set; }
        public decimal Opacity { get; set; }
    }
    public class TimelineChartData
    {
        public string title { get; set; }
        public List<State> measures { get; set; }
        public List<Point> markers { get; set; }
        public bool showTicks { get; set; }
    }
}

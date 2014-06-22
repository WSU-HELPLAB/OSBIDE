using System.Collections.Generic;

namespace OSBIDE.Data.DomainObjects
{
    public class Point
    {
        public string Name { get; set; }
        public int Position { get; set; }
    }
    public class TimelineChartData
    {
        public const int DEFAULT_TIMEOUT = 3;

        public string title { get; set; }
        public List<Point> measures { get; set; }
        public List<int> markers { get; set; }
        public bool showTicks { get; set; }
    }
}

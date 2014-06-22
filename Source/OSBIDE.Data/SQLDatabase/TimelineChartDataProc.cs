using System.Collections.Generic;
using OSBIDE.Data.DomainObjects;

namespace OSBIDE.Data.SQLDatabase
{
    public class TimelineChartDataProc
    {
        public static List<TimelineChartData> Get(TimeScale timescale, int? timeout)
        {
            var timeoutVal = timeout.HasValue ? timeout.Value : TimelineChartData.DEFAULT_TIMEOUT;

            return new List<TimelineChartData>
            {
                new TimelineChartData{
                                        title="Joe Smith",
                                        measures=new List<Point>
                                        {
                                            new Point{Name="idle",Position=220},
                                            new Point{Name="edit",Position=270},
                                            new Point{Name="idle",Position=370},
                                            new Point{Name="debug",Position=389},
                                            new Point{Name="run",Position=500},
                                        },
                                        markers=new List<int>
                                        {
                                            285,388
                                            //new Point{Name="QT", Position=285},
                                            //new Point{Name="QA", Position=388}
                                        }
                                    },
                new TimelineChartData{
                                        title="Linda Ann",
                                        measures=new List<Point>
                                        {
                                            new Point{Name="idle",Position=120},
                                            new Point{Name="edit",Position=320},
                                        },
                                        markers=new List<int>
                                        {
                                            128
                                            //new Point{Name="PP", Position=128}
                                        }
                                    },
                new TimelineChartData{
                                        title="Bob Fan",
                                        measures=new List<Point>
                                        {
                                            new Point{Name="edit",Position=26},
                                            new Point{Name="idle",Position=33},
                                            new Point{Name="debug",Position=78},
                                        },
                                        markers=new List<int>
                                        {
                                            36
                                            //new Point{Name="QQ", Position=36}
                                        },
                                        showTicks=true,
                                    },
            };
        }
    }
}

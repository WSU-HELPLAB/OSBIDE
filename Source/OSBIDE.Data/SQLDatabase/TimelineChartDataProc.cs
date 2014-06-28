using System.Collections.Generic;
using OSBIDE.Data.DomainObjects;

namespace OSBIDE.Data.SQLDatabase
{
    public class TimelineChartDataProc
    {
        private static List<TimelineChartData> StaticDataDayScale
        {
            get
            {
                #region a list of startic day scaled data
                return new List<TimelineChartData>
                                {
                                    new TimelineChartData{
                                                            title="Joe Smith",
                                                            measures=new List<State>
                                                            {
                                                                new State{Name="idle",StartPoint=0,EndPoint=220},
                                                                new State{Name="nonidle",StartPoint=220,EndPoint=270},
                                                                new State{Name="idle",StartPoint=270,EndPoint=370},
                                                                new State{Name="nonidle",StartPoint=370,EndPoint=389},
                                                                new State{Name="idle",StartPoint=389,EndPoint=500},
                                                            },
                                                            markers = new List<Point>()
                                                        },
                                    new TimelineChartData{
                                                            title="Linda Ann",
                                                            measures=new List<State>
                                                            {
                                                                new State{Name="idle",StartPoint=0,EndPoint=120},
                                                                new State{Name="nonidle",StartPoint=120,EndPoint=320},
                                                            },
                                                            markers = new List<Point>()
                                                        },
                                    new TimelineChartData{
                                                            title="Bob Fan",
                                                            measures=new List<State>
                                                            {
                                                                new State{Name="nonidle",StartPoint=0,EndPoint=26},
                                                                new State{Name="idle",StartPoint=0,EndPoint=33},
                                                                new State{Name="nonidle",StartPoint=0,EndPoint=78},
                                                            },
                                                            markers = new List<Point>(),
                                                            showTicks=true,
                                                        },
                                };
                #endregion
            }
        }

        private static List<TimelineChartData> StaticDataHourScale
        {
            get
            {
                #region a list of static hour scaled data
                return new List<TimelineChartData>
                                {
                                    new TimelineChartData{
                                                            title="Joe Smith",
                                                            measures=new List<State>
                                                            {
                                                                new State{Name="run",StartPoint=0,EndPoint=220,Opacity=.2m},
                                                                new State{Name="idle",StartPoint=220,EndPoint=270,Opacity=.5m},
                                                                new State{Name="edit",StartPoint=270,EndPoint=370,Opacity=.3m},
                                                                new State{Name="debug",StartPoint=370,EndPoint=389,Opacity=.6m},
                                                                new State{Name="eidt",StartPoint=389,EndPoint=500,Opacity=.9m},
                                                            },
                                                            markers=new List<Point>
                                                            {
                                                                new Point{Name="QT",Position=285},
                                                                new Point{Name="QA",Position=388}
                                                            }
                                                        },
                                    new TimelineChartData{
                                                            title="Linda Ann",
                                                            measures=new List<State>
                                                            {
                                                                new State{Name="run",StartPoint=0,EndPoint=120,Opacity=.8m},
                                                                new State{Name="edit",StartPoint=120,EndPoint=320,Opacity=.6m},
                                                            },
                                                            markers=new List<Point>
                                                            {
                                                                new Point{Name="PP",Position=128}
                                                            }
                                                        },
                                    new TimelineChartData{
                                                            title="Bob Fan",
                                                            measures=new List<State>
                                                            {
                                                                new State{Name="edit",StartPoint=0,EndPoint=26,Opacity=.9m},
                                                                new State{Name="idle",StartPoint=26,EndPoint=33,Opacity=.3m},
                                                                new State{Name="run",StartPoint=33,EndPoint=78,Opacity=.5m},
                                                            },
                                                            markers=new List<Point>
                                                            {
                                                                new Point{Name="QQ",Position=36}
                                                            },
                                                            showTicks=true,
                                                        },
                                };
                #endregion
            }
        }

        private static List<TimelineChartData> StaticDataMinuteScale
        {
            get
            {
                #region a list of static minute scaled data
                return new List<TimelineChartData>
                            {
                                new TimelineChartData{
                                                        title="Joe Smith",
                                                        measures=new List<State>
                                                        {
                                                            new State{Name="idle",StartPoint=0,EndPoint=220},
                                                            new State{Name="run_unknown",StartPoint=220,EndPoint=270},
                                                            new State{Name="edit_syn_n_sem_u",StartPoint=270,EndPoint=370},
                                                            new State{Name="edit_syn_u_sem_u",StartPoint=370,EndPoint=389},
                                                            new State{Name="idle",StartPoint=389,EndPoint=500},

                                                        },
                                                        markers=new List<Point>
                                                        {
                                                            new Point{Name="QT",Position=285},
                                                            new Point{Name="QA",Position=388}
                                                        }
                                                    },
                                new TimelineChartData{
                                                        title="Linda Ann",
                                                        measures=new List<State>
                                                        {
                                                            new State{Name="run_unknown",StartPoint=0,EndPoint=200},
                                                            new State{Name="edit_syn_n_sem_u",StartPoint=200,EndPoint=320},
                                                            new State{Name="edit_syn_u_sem_u",StartPoint=320,EndPoint=500},
                                                        },
                                                        markers=new List<Point>
                                                        {
                                                            new Point{Name="PP",Position=128}
                                                        }
                                                    },
                                new TimelineChartData{
                                                        title="Bob Fan",
                                                        measures=new List<State>
                                                        {
                                                            new State{Name="idle",StartPoint=0,EndPoint=120},
                                                            new State{Name="run_unknown",StartPoint=120,EndPoint=230},
                                                            new State{Name="edit_syn_n_sem_u",StartPoint=230,EndPoint=355},
                                                            new State{Name="edit_syn_u_sem_u",StartPoint=355,EndPoint=389},
                                                            new State{Name="idle",StartPoint=389,EndPoint=500},
                                                        },
                                                        markers=new List<Point>
                                                        {
                                                            new Point{Name="QQ",Position=36}
                                                        },
                                                        showTicks=true,
                                                    },
                                         };
                #endregion
            }
        }
        public static List<TimelineChartData> Get(TimeScale timescale, int? timeout, bool grayscale)
        {
            var timeoutVal = timeout.HasValue ? timeout.Value : VisualizationParams.DEFAULT_TIMEOUT;

            var chartData = timescale == TimeScale.Days ? StaticDataDayScale
                          : timescale == TimeScale.Hours ? StaticDataHourScale : StaticDataMinuteScale;

            if (grayscale)
            {
                chartData.ForEach(d => d.measures.ForEach(m => m.Name = string.Format("{0} {1}", m.Name, "color_grayscale")));
            }
            else
            {
                chartData.ForEach(d => d.measures.ForEach(m => m.Name = string.Format("{0} {1}", m.Name, "color_default")));
            }

            return chartData;
        }
    }
}

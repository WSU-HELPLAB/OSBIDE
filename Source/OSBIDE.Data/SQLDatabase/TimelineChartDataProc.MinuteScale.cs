﻿using System;
using System.Collections.Generic;
using System.Linq;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase.Edmx;
using OSBIDE.Library.Events;

namespace OSBIDE.Data.SQLDatabase
{
    public partial class TimelineChartDataProc
    {
        private static List<TimelineChartData> GetTimelineDataMinuteScale(DateTime? dateFrom, DateTime? dateTo, IEnumerable<int> userIds, bool grayscale, DateTime unspecifiedDate, TimeScale timescale, int timeoutVal)
        {
            using (var context = new OsbideProcs())
            {
                var timeFrom = dateFrom.HasValue ? dateFrom.Value : unspecifiedDate;
                var timeTo = dateTo.HasValue ? dateTo.Value : unspecifiedDate;

                // get raw data from database and sort by user and event time
                var rawR = (from e in context.GetStateMachineEvents(timeFrom, timeTo, string.Join(",", userIds))
                            select e).ToList().OrderBy(x => x.SenderId).ThenBy(x => x.EventDate);

                // set the actual data range
                if (timeFrom == unspecifiedDate) timeFrom = rawR.Min(x => x.EventDate);
                if (timeTo == unspecifiedDate) timeTo = rawR.Max(x => x.EventDate);

                var totalMinutes = (int)(timeTo - timeFrom).TotalMinutes;

                // chart data timeline
                var chartData = new List<TimelineChartData>();
                foreach (var r in rawR)
                {
                    //AC TODO: On solution name change, revert back to syn_u_sym_u (Reminder: ask Chris at next meeting)

                    // processing the flat events and compose a list of programming states for each user
                    // the composed list of programming states are ordered by the event time
                    // so the output is a collection of state timelines for the list of users
                    var currEventTime = r.EventDate;
                    var currEventPoint = (currEventTime - timeFrom).TotalMinutes;

                    // init local variables
                    var prevStateName = ProgrammingState.edit_syn_u_sem_u;

                    // process one user at a time
                    var userData = chartData.SingleOrDefault(x => x.UserId == r.SenderId);

                    // a new user's timeline?
                    if (userData == null)
                    {
                        // yes, create a new timeline entry
                        #region a new user timeline

                        // since each bullet svg is drawn independently
                        // the high level information needs to be passed into each svg drawing routine
                        userData = new TimelineChartData
                        {
                            UserId = r.SenderId,
                            title = string.Format("{0} {1}", r.FirstName, r.LastName),
                            markers = new List<Point>(),
                            measures = new List<State>(),
                        };

                        // is the first event in idle timeout range?
                        if ((currEventTime - timeFrom).TotalMinutes > timeoutVal)
                        {
                            // no, insert an idle state from the beginning to now
                            prevStateName = ProgrammingState.idle;
                            var uiproperties = TimelineStateDictionaries.UIProperties[prevStateName];
                            var idleState = new State
                            {
                                ProgrammingState = prevStateName,
                                Name = uiproperties.Label,
                                CssClass = grayscale ? uiproperties.CssGray : uiproperties.Css,
                                StartTime = timeFrom,
                                ShiftedStartTime = timeFrom,
                                StartPoint = 0,
                                EndTime = currEventTime,
                                EndPoint = currEventPoint,
                            };
                            userData.measures.Add(idleState);
                        }
                        else
                        {
                            // yes, insert a default programming state
                            prevStateName = ProgrammingState.edit_syn_u_sem_u;
                            var uiproperties = TimelineStateDictionaries.UIProperties[prevStateName];
                            var defaultState = new State
                            {
                                ProgrammingState = prevStateName,
                                Name = uiproperties.Label,
                                CssClass = grayscale ? uiproperties.CssGray : uiproperties.Css,
                                StartTime = timeFrom,
                                ShiftedStartTime = timeFrom,
                                StartPoint = 0,
                                EndTime = timeTo,
                                EndPoint = totalMinutes,
                            };
                            userData.measures.Add(defaultState);
                        }

                        chartData.Add(userData);

                        #endregion
                    }

                    // is this a social event?
                    if (!string.IsNullOrWhiteSpace(r.MarkerType))
                    {
                        // yes, add social media event marker
                        userData.markers.Add(new Point { Name = r.MarkerType, Position = currEventPoint, TickTime = currEventTime });
                    }
                    else
                    {
                        // no, transition to next state based on the previous state
                        // non-social event could be a state transition trigger

                        // first to check the timespan between current and previous non-idle state events

                        #region insert an idle state if the timespan between this event and previous event is too long

                        var prevState = userData.measures.Last();
                        var prevEventTime = prevState.ShiftedStartTime;
                        prevStateName = prevState.ProgrammingState;

                        if ((currEventTime - prevEventTime).TotalMinutes > timeoutVal)
                        {
                            if (prevStateName == ProgrammingState.idle)
                            {
                                // extend the initial idle state to avoid the extra rectangle
                                prevState.EndTime = currEventTime;
                                prevState.EndPoint = currEventPoint;
                            }
                            else
                            {
                                #region add a new idle state

                                prevStateName = ProgrammingState.idle;
                                var uiproperties = TimelineStateDictionaries.UIProperties[prevStateName];
                                var idleState = new State
                                {
                                    ProgrammingState = prevStateName,
                                    Name = uiproperties.Label,
                                    CssClass = grayscale ? uiproperties.CssGray : uiproperties.Css,
                                    StartTime = prevEventTime.AddMinutes(timeoutVal),
                                    StartPoint = (prevEventTime - timeFrom).TotalMinutes + timeoutVal,
                                    EndTime = currEventTime,
                                    EndPoint = currEventPoint,
                                };
                                userData.measures.Add(idleState);

                                // terminate previous non-idle state
                                prevState.EndTime = idleState.StartTime;
                                prevState.EndPoint = idleState.StartPoint;

                                #endregion
                            }
                        }

                        #endregion

                        // resolve next state name from current state and current event
                        #region get next programming state from state dictionaries

                        var nextStateName = prevStateName;
                        if (string.Compare(r.LogType, "BuildEvent", true) == 0)
                        {
                            // the key check should be removed once the state dictionaries cover all scenarios
                            nextStateName = r.BuildErrorLogId.HasValue
                                            ? TimelineStateDictionaries.NextStateForBuildWithError[prevStateName]
                                            : TimelineStateDictionaries.NextStateForBuildWithoutError[prevStateName];
                        }
                        else if (string.Compare(r.LogType, "DebugEvent", true) == 0 && r.ExecutionAction.HasValue)
                        {
                            // the key check should be removed once the state dictionaries cover all scenarios
                            //TODO: Code Review 2014-07-14: Change ProgrammingState.idle to prevStateName?
                            nextStateName = r.ExecutionAction == (int)DebugActions.StartWithoutDebugging
                                            ? TimelineStateDictionaries.NextStateForStartWithoutDebugging[ProgrammingState.idle]
                                            : TimelineStateDictionaries.NextStateForDebug[ProgrammingState.idle];
                        }
                        else if (TimelineStateDictionaries.EditorEvents.Any(x => string.Compare(x, r.LogType, true) == 0))
                        {
                            // the key check should be removed once the state dictionaries cover all scenarios
                            nextStateName = TimelineStateDictionaries.NextStateForEditorEvent[prevStateName];
                        }

                        #endregion

                        // terminate previous state and append the next state if it's changed
                        if (nextStateName != prevStateName)
                        {
                            #region append new state

                            // idle state is instantiated with a valid termination date
                            // when idle state is created, it also terminates the previous state (timeout)
                            if (prevState.EndTime == timeTo)
                            {
                                // terminate previous non-idle state
                                prevState.EndTime = currEventTime;
                                prevState.EndPoint = currEventPoint;
                            }

                            var uiproperties = TimelineStateDictionaries.UIProperties[nextStateName];
                            userData.measures.Add(new State
                            {
                                // new state
                                ProgrammingState = nextStateName,
                                Name = uiproperties.Label,
                                CssClass = grayscale ? uiproperties.CssGray : uiproperties.Css,
                                StartTime = currEventTime,
                                ShiftedStartTime = currEventTime,
                                StartPoint = currEventPoint,
                                EndTime = timeTo,
                                EndPoint = totalMinutes,
                            });

                            #endregion
                        }
                        else
                        {
                            // update continous state start time so it won't timeout to idle state
                            prevState.ShiftedStartTime = currEventTime;
                        }
                    }
                }

                #region append idle state to the end if necessary

                chartData.ForEach(x =>
                {
                    var lastMeasure = x.measures.Last();
                    if (lastMeasure.EndTime == timeTo && (timeTo - lastMeasure.ShiftedStartTime).TotalMinutes > timeoutVal)
                    {
                        // prepare a new idle state for the end
                        var uiproperties = TimelineStateDictionaries.UIProperties[ProgrammingState.idle];
                        var idleState = new State
                        {
                            ProgrammingState = ProgrammingState.idle,
                            Name = uiproperties.Label,
                            CssClass = grayscale ? uiproperties.CssGray : uiproperties.Css,
                            StartTime = lastMeasure.StartTime.AddMinutes(timeoutVal),
                            StartPoint = lastMeasure.StartPoint + timeoutVal,
                            EndTime = lastMeasure.EndTime,
                            EndPoint = lastMeasure.EndPoint,
                        };
                        x.measures.Add(idleState);

                        // terminate the lastMeasure state
                        lastMeasure.EndTime = idleState.StartTime;
                        lastMeasure.EndPoint = idleState.StartPoint;
                    }
                });

                #endregion

                // only show horizontal x-axis for the last series
                if (chartData.Count > 0) chartData.Last().showTicks = true;

                return chartData;
            }
        }
    }
}
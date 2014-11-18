using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Terminal.Models
{
    class StudentTimeline
    {
        public int OsbideId { get; set; }
        public int StudentId { get; set; }
        public List<TimelineState> RawStates { get; set; }
        public Dictionary<string, double> Grades { get; set; }
        private Dictionary<string, TimelineState> _aggregateStates { get; set; }

        public StudentTimeline()
        {
            RawStates = new List<TimelineState>();
            Grades = new Dictionary<string, double>();
            _aggregateStates = new Dictionary<string, TimelineState>();
        }

        //returns aggregate information for a given state
        public TimelineState GetAggregateState(string key)
        {
            //check to see if we need to load the state
            if(_aggregateStates.Count == 0)
            {
                foreach (TimelineState state in RawStates)
                {
                    if (_aggregateStates.ContainsKey(state.State) == false)
                    {
                        _aggregateStates.Add(state.State, new TimelineState()
                        {
                            State = state.State,
                            IsSocialEvent = state.IsSocialEvent,
                            StartTime = DateTime.MinValue,
                            EndTime = DateTime.MinValue
                        });
                    }

                    //social states are instant, so we'll just increment a single tick to mark an individual event
                    if (state.IsSocialEvent == false)
                    {
                        TimeSpan difference = state.EndTime - state.StartTime;

                        //currently, there's a bug that causes some events to create negaitve time.  Ignore these events.
                        if(difference.TotalSeconds > 0)
                        {
                            _aggregateStates[state.State].EndTime += difference;
                        }
                    }
                    else
                    {
                        _aggregateStates[state.State].EndTime += new TimeSpan(0, 0, 1);
                    }
                }
            }
            if(_aggregateStates.ContainsKey(key))
            {
                return _aggregateStates[key];
            }
            else
            {
                _aggregateStates[key] = new TimelineState() { };
                return _aggregateStates[key];
            }
        }

        //resets aggregate state information 
        public void ClearAggregateStates()
        {
            _aggregateStates.Clear();
        }
    }
}

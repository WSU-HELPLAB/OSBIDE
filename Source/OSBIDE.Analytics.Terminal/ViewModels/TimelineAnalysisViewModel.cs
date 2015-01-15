using OSBIDE.Analytics.Terminal.Models;
using OSBIDE.Library.CSV;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Terminal.ViewModels
{
    public class TimelineAnalysisViewModel
    {
        public Dictionary<int, StudentTimeline> Timeline { get; set; }

        private OsbideContext _db { get; set; }

        public TimelineAnalysisViewModel()
        {
            _db = OsbideContext.DefaultWebConnection;
            Timeline = new Dictionary<int, StudentTimeline>();
        }

        /// <summary>
        /// Returns all states present in the currently loaded Timeline
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllStates()
        {
            Dictionary<string, string> allStates = new Dictionary<string,string>();
            foreach(var user in Timeline)
            {
                foreach(var state in user.Value.RawStates)
                {
                    allStates[state.State] = state.State;
                }
            }
            List<string> result = allStates.Keys.ToList();
            result.Sort();
            return result;
        }

        /// <summary>
        /// returns a list of all grades present in the currently loaded timeline
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllGrades()
        {
            Dictionary<string, string> allGrades = new Dictionary<string,string>();
            foreach (var user in Timeline.Values)
            { 
                foreach(var grade in user.Grades)
                {
                    allGrades[grade.Key] = grade.Key;
                }
            }
            List<string> result = allGrades.Keys.ToList();
            result.Sort();
            return result;
        }

        /// <summary>
        /// Attaches grade data to the currently loaded timeline
        /// </summary>
        public void AttachGrades()
        {
            //pull grades from DB
            var query = from user in _db.Users
                        join grade in _db.StudentGrades on user.InstitutionId equals grade.StudentId
                        where Timeline.Keys.Contains(user.Id)
                        select new { UserId = user.Id, StudentId = user.InstitutionId, Deliverable = grade.Deliverable, Grade = grade.Grade };

            //add to timeline students
            foreach(var item in query)
            {
                if(Timeline.ContainsKey(item.UserId) == true)
                {
                    if(Timeline[item.UserId].Grades.ContainsKey(item.Deliverable) == false)
                    {
                        Timeline[item.UserId].Grades.Add(item.Deliverable, item.Grade);
                    }
                    Timeline[item.UserId].Grades[item.Deliverable] = item.Grade;
                }
            }
        }

        //adds normalized programming state metrics to each user
        public void NormalizeProgrammingStates()
        {
            /*programming states: 
             * ?? - Unknown
             * Y? - SynYSemU
             * YN - SynYSemN
             * N? - SynNSemU
             * NN - SynNSemN
             * D? - Debugging SemU
             * DN - Debugging SemN
             * RN - Run SemU
             * R? - Run SemN
             * R/ - Run SynNSemU
             * 
             * Algorithm: sum states total time, normalize based on total time spent in each state
             */
            string[] intersting_states = {"??", "Y?", "YN", "N?", "NN", "D?", "DN", "RN", "R?", "R/"};
            foreach(StudentTimeline timeline in Timeline.Values)
            {
                TimeSpan total_time = new TimeSpan(0, 0, 0);

                //pass #1: find total time
                foreach(string state in intersting_states)
                {
                    TimelineState aggregate = timeline.GetAggregateState(state);
                    total_time += aggregate.TimeInState;
                }

                //add total time to states
                timeline.GetAggregateState("normalized_total_time").StartTime = DateTime.MinValue;
                timeline.GetAggregateState("normalized_total_time").EndTime = DateTime.MinValue + total_time;
                TimelineState totalState = new TimelineState()
                {
                    State = "normalized_total_time",
                    StartTime = DateTime.MinValue,
                    EndTime = timeline.GetAggregateState("normalized_total_time").EndTime
                };
                timeline.RawStates.Add(totalState);

                //pass #2: normalize
                foreach(string state in intersting_states)
                {
                    string normilzedKey = string.Format("normalized_{0}", state);
                    TimelineState aggregate = timeline.GetAggregateState(state);
                    TimelineState normalizedState = new TimelineState()
                    {
                        State = normilzedKey,
                        NormalizedTimeInState = (aggregate.TimeInState.TotalSeconds / total_time.TotalSeconds)*100,
                    };

                    //add back to student
                    timeline.GetAggregateState(normilzedKey).NormalizedTimeInState =  normalizedState.NormalizedTimeInState;
                    timeline.RawStates.Add(normalizedState);
                }

            }
        }

        /// <summary>
        /// Builds transition counts for each loaded timeline
        /// </summary>
        public void ProcessTransitions()
        {
            foreach(StudentTimeline student in Timeline.Values)
            {
                student.AggregateTransitions();
            }
        }

        /// <summary>
        /// Returns all transitions present in the loaded timeline
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<string, string>> GetAllTransitions()
        {
            Dictionary<KeyValuePair<string, string>, int> transitions = new Dictionary<KeyValuePair<string, string>, int>();
            foreach(StudentTimeline timeline in Timeline.Values)
            {
                foreach(var transition in timeline.Transitions)
                {
                    transitions[transition.Key] = 0;
                }
            }
            return transitions.Keys.ToList();
        }

        /// <summary>
        /// Writes transition counts to a CSV file
        /// </summary>
        /// <param name="fileName"></param>
        public void WriteTransitionsToCsv(string fileName)
        {
            //write results to file
            CsvWriter writer = new CsvWriter();

            //add header row
            writer.AddToCurrentLine("User ID");
            var query = from item  in GetAllTransitions()
                        select new {
                            Key = item.Key, 
                            Value = item.Value, 
                            AsString = string.Format("{0};{1}", item.Key, item.Value),
                            Kvp = item
                        };
            var transitions = query.OrderBy(q => q.AsString).ToList();
            foreach (var transition in transitions)
            {
                writer.AddToCurrentLine(transition.AsString);
            }

            //add grades if loaded
            List<string> grades = GetAllGrades();
            foreach (string grade in grades)
            {
                writer.AddToCurrentLine(grade);
            }

            writer.CreateNewRow();

            //add data rows
            foreach (var item in Timeline.Values)
            {
                writer.AddToCurrentLine(item.OsbideId.ToString());

                //add state information
                foreach (var transition in transitions)
                {
                    if(item.Transitions.ContainsKey(transition.Kvp) == true)
                    {
                        writer.AddToCurrentLine(item.Transitions[transition.Kvp].ToString());
                    }
                    else
                    {
                        writer.AddToCurrentLine("0");
                    }
                }

                //add grade information
                foreach (string grade in grades)
                {
                    if (item.Grades.ContainsKey(grade) == true)
                    {
                        writer.AddToCurrentLine(item.Grades[grade].ToString());
                    }
                    else
                    {
                        writer.AddToCurrentLine("0");
                    }
                }
                writer.CreateNewRow();
            }

            //write to file
            using (TextWriter tw = File.CreateText(fileName))
            {
                tw.Write(writer.ToString());
            }
        }

        /// <summary>
        /// Writes time in state information to a CSV file
        /// </summary>
        /// <param name="fileName"></param>
        public void WriteTimeInStateToCsv(string fileName)
        {
            //write results to file
            CsvWriter writer = new CsvWriter();

            //add header row
            writer.AddToCurrentLine("User ID");
            List<string> states = GetAllStates();
            foreach (string state in states)
            {
                writer.AddToCurrentLine(state);
            }

            //add grades if loaded
            List<string> grades = GetAllGrades();
            foreach(string grade in grades)
            {
                writer.AddToCurrentLine(grade);
            }

            writer.CreateNewRow();

            //add data rows
            foreach (var item in Timeline.Values)
            {
                writer.AddToCurrentLine(item.OsbideId.ToString());

                //add state information
                foreach (string state in states)
                {
                    //normalized states will use the "NormalizedTimeInState" property.
                    //non-normalized states will use the "MillisecondsInState" property.
                    if(item.GetAggregateState(state).NormalizedTimeInState > 0)
                    {
                        writer.AddToCurrentLine(item.GetAggregateState(state).NormalizedTimeInState.ToString("0.0000000"));
                    }
                    else
                    {
                        writer.AddToCurrentLine(item.GetAggregateState(state).TimeInState.TotalSeconds.ToString());
                    }
                    
                }

                //add grade information
                foreach (string grade in grades)
                {
                    if(item.Grades.ContainsKey(grade) == true)
                    {
                        writer.AddToCurrentLine(item.Grades[grade].ToString());
                    }
                    else
                    {
                        writer.AddToCurrentLine("0");
                    }
                }
                writer.CreateNewRow();
            }

            //write to file
            using (TextWriter tw = File.CreateText(fileName))
            {
                tw.Write(writer.ToString());
            }
        }

        private Dictionary<int, StudentTimeline> ParseTimeline(string fileName, Dictionary<int, StudentTimeline> userStates)
        {
            //get raw data from CSV file
            List<List<string>> rawData = new List<List<string>>();
            using (FileStream fs = File.OpenRead(fileName))
            {
                CsvReader csv = new CsvReader(fs);
                rawData = csv.Parse();
            }

            //convert raw data into object form
            foreach (List<string> pieces in rawData)
            {
                //pull user ID
                int userId = -1;
                Int32.TryParse(pieces[0], out userId);

                if(userStates.ContainsKey(userId) == false)
                {
                    userStates.Add(userId, new StudentTimeline());
                }

                foreach (string entry in pieces)
                {
                    //split data elements
                    string[] parts = entry.Split(new Char[] { ';' });

                    //ignore first record, which is user ID
                    if (parts.Length < 2)
                    {
                        continue;
                    }

                    //build current state
                    TimelineState currentState = new TimelineState();
                    currentState.State = parts[0];
                    currentState.UserId = userId;
                    DateTime tempDate = DateTime.MinValue;
                    DateTime.TryParse(parts[1], out tempDate);
                    currentState.StartTime = tempDate;


                    //two items = social event
                    if (parts.Length == 2)
                    {
                        currentState.IsSocialEvent = true;

                        //social events do not have an end time
                        currentState.EndTime = currentState.StartTime;
                    }
                    else
                    {
                        tempDate = DateTime.MinValue;
                        DateTime.TryParse(parts[2], out tempDate);
                        currentState.EndTime = tempDate;
                    }

                    //add to dictionary
                    userStates[userId].OsbideId = userId;
                    userStates[userId].RawStates.Add(currentState);
                }
            }
            return userStates;
        }

        /// <summary>
        /// Loads a new timeline from the supplied file
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadTimeline(string fileName)
        {
            Timeline = ParseTimeline(fileName, new Dictionary<int,StudentTimeline>());
        }

        /// <summary>
        /// Appends a new file's timeline onto an already loaded timeline
        /// </summary>
        /// <param name="fileName"></param>
        public void AppendTimeline(string fileName)
        {
            Timeline = ParseTimeline(fileName, Timeline);
        }
    }
}

using OSBIDE.Analytics.Terminal.Models;
using OSBIDE.Library.CSV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Terminal.ViewModels
{
    class TimelineAnalysisViewModel
    {
        public Dictionary<int, List<TimelineState>> ParseTimeline(string fileName)
        {
            //get raw data from CSV file
            List<List<string>> rawData = new List<List<string>>();
            using (FileStream fs = File.OpenRead(fileName))
            {
                CsvReader csv = new CsvReader(fs);
                rawData = csv.Parse();
            }

            //convert raw data into object form
            Dictionary<int, List<TimelineState>> userStates = new Dictionary<int, List<TimelineState>>();
            foreach (List<string> pieces in rawData)
            {
                //pull user ID
                int userId = -1;
                Int32.TryParse(pieces[0], out userId);
                userStates.Add(userId, new List<TimelineState>());

                foreach (string entry in pieces)
                {
                    //split data elements
                    string[] parts = entry.Split(new Char[] { ';' });

                    //ignore first record, which is user ID
                    if(parts.Length < 2)
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
                    userStates[userId].Add(currentState);
                }
            }

            return userStates;
        }
    }
}

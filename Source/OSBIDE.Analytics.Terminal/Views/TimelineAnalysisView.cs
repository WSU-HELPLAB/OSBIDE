using OSBIDE.Analytics.Terminal.Models;
using OSBIDE.Analytics.Terminal.ViewModels;
using OSBIDE.Library.CSV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Terminal.Views
{
    class TimelineAnalysisView
    {
        private enum MenuOption
        {
            ChangeDirectory = 1,
            ListFiles,
            LoadFile,
            AppendFile,
            LoadGrades,
            FilterGrades,
            NormalizeProgrammingStates,
            CountTransitions,
            ConstructMarkovModel,
            ProcessMarkovModel,
            WriteAggregateToFile,
            WriteTransitionsToFile,
            WriteDataToCsv,
            Exit
        }

        private TimelineAnalysisViewModel Vm { get; set; }

        public TimelineAnalysisView()
        {
            Vm = new TimelineAnalysisViewModel();
        }

        /// <summary>
        /// Constructs a new Markov model using currently loaded data
        /// </summary>
        /// <param name="vm"></param>
        private void ConstructMarkovModel(TimelineAnalysisViewModel vm)
        {
            //step 1: build states
            vm.BuildDefaultMarkovStates();

            //construct idle sequences for markov learning
            Dictionary<string, int> idleSequences = vm.GetIdleTransitionSequence();

            //get learning sequence
            List<List<int>> trainingSequence = new List<List<int>>();
            foreach(string sequence in idleSequences.Keys)
            {
                //remove all singletons from consideration
                if(idleSequences[sequence] > 1)
                {
                    List<int> toAdd = sequence.Split('_').Select(c => Convert.ToInt32(c)).ToList();
                    trainingSequence.Add(toAdd);
                }
            }

            //step 2: construct markov model
            //vm.BuildMarkovModel(trainingSequence);
        }

        /*
         * What I want to do:
         *  For each assignment:
         *      figure out common se
         * */

        /// <summary>
        /// Processes currently loaded data through the currently loaded markov model
        /// </summary>
        /// <param name="vm"></param>
        private void ProcessMarkovModel(TimelineAnalysisViewModel vm)
        {
            
            CsvWriter writer = new CsvWriter();

            Dictionary<string, int> idleSequences = vm.GetIdleTransitionSequence();
            Dictionary<string, double> probabilities = new Dictionary<string, double>();// vm.GetMarkovProbabilities(idleSequences.Keys.ToList());
            writer = new CsvWriter();
            foreach (KeyValuePair<string, double> kvp in probabilities)
            {
                writer.AddToCurrentLine(kvp.Key);
                writer.AddToCurrentLine(kvp.Value.ToString());
                writer.CreateNewRow();
            }
            using (TextWriter tw = File.CreateText("probabilites.csv"))
            {
                tw.Write(writer.ToString());
            }
            
            
            Console.WriteLine("Processing idle sequences...");
            foreach (KeyValuePair<string, int> kvp in idleSequences)
            {
                writer.AddToCurrentLine(kvp.Key);
                writer.AddToCurrentLine(kvp.Value.ToString());
                writer.CreateNewRow();
            }
            using (TextWriter tw = File.CreateText(string.Format("idle_sequences.csv")))
            {
                tw.Write(writer.ToString());
            }

            /*

            //step 3: get and determine probability of all possible transitions from the loaded data
            for (int i = 2; i < 50; i++)
            {
                Dictionary<string, int> transitions = vm.GetAllTransitionCombinations(i);
                Dictionary<string, double> probabilities = vm.GetMarkovProbabilities(transitions.Keys.ToList());

                //write raw counts to CSV
                writer = new CsvWriter();
                foreach(KeyValuePair<string, int> kvp in transitions)
                {
                    writer.AddToCurrentLine(kvp.Key);
                    writer.AddToCurrentLine(kvp.Value.ToString());
                    writer.CreateNewRow();
                }
                using (TextWriter tw = File.CreateText(string.Format("counts_{0}.csv", i)))
                {
                    tw.Write(writer.ToString());
                }

                //write probabilities to CSV
                writer = new CsvWriter();
                foreach(KeyValuePair<string, double> kvp in probabilities)
                {
                    writer.AddToCurrentLine(kvp.Key);
                    writer.AddToCurrentLine(kvp.Value.ToString());
                    writer.CreateNewRow();
                }
                using(TextWriter tw = File.CreateText(string.Format("probabilites_{0}.csv", i)))
                {
                    tw.Write(writer.ToString());
                }
             }
             * */
        }

        /// <summary>
        /// Filters loaded data based on a given criteria set
        /// </summary>
        /// <param name="vm"></param>
        private void FilterStudentsByGrade(TimelineAnalysisViewModel vm)
        {
            //present options to filter by
            List<string> gradingCriteria = vm.GetAllGrades();
            Console.WriteLine("***Grading Categories***");
            for(int i = 0; i < gradingCriteria.Count; i++)
            {
                Console.WriteLine("{0}: {1}", i, gradingCriteria[i]);
            }
            Console.Write("Select category: ");
            string categoryStr = Console.ReadLine();

            //ask for min/max filter
            Console.Write("Enter max score [100%]: ");
            string maxScoreStr = Console.ReadLine();
            Console.Write("Enter min score [0%]: ");
            string minScoreStr = Console.ReadLine();

            //parse strings, send off to VM for processing
            int category = -1;
            double maxScore = 200;
            double minScore = 0;
            if(Int32.TryParse(categoryStr, out category) && category > -1)
            {
                if(Double.TryParse(maxScoreStr, out maxScore) == false)
                {
                    maxScore = 200;
                }
                if (Double.TryParse(minScoreStr, out minScore) == false)
                {
                    minScore = 0;
                }

                int result = vm.FilterByGrade(gradingCriteria[category], minScore, maxScore);
                Console.WriteLine("{0} entries removed from dataset.", result);
            }
            else
            {
                Console.WriteLine("Error parsing user input.");
            }
        }

        private string GetFile()
        {
            int counter = 1;

            //enumerate files in working directory
            List<string> files = Directory.EnumerateFiles(".", "*.csv").ToList();
            foreach (string file in files)
            {
                Console.WriteLine("{0}. {1}", counter, file);
                counter++;
            }
            Console.Write(">> ");
            int fileToRead = -1;
            string user_input = Console.ReadLine();
            Int32.TryParse(user_input, out fileToRead);

            //ensure valid selection
            if (fileToRead > 0 && fileToRead < counter)
            {
                return files[fileToRead - 1];
            }
            return "";
        }

        private void LoadFile()
        {
            string file = GetFile();
            if (file.Length > 0)
            {
                Vm.LoadTimeline(file);
            }
        }

        private void AppendFile()
        {
            string file = GetFile();
            if (file.Length > 0)
            {
                Vm.AppendTimeline(file);
            }
        }

        public void Run()
        {
            int userChoice = 0;
            while (userChoice != (int)MenuOption.Exit)
            {
                Console.WriteLine((int)MenuOption.ChangeDirectory + ". Change working directory");
                Console.WriteLine((int)MenuOption.ListFiles + ". List all files in current directory");
                Console.WriteLine((int)MenuOption.LoadFile + ". Load file (clears existing timeline data)");
                Console.WriteLine((int)MenuOption.AppendFile + ". Append data to existing timeline data");
                Console.WriteLine((int)MenuOption.LoadGrades + ". Attach grade data to students");
                Console.WriteLine((int)MenuOption.FilterGrades + ". Filter students by grade");
                Console.WriteLine((int)MenuOption.NormalizeProgrammingStates + ". Normalize programming states");
                Console.WriteLine((int)MenuOption.CountTransitions + ". Count transitions");
                Console.WriteLine((int)MenuOption.ConstructMarkovModel + ". Construct Markov model");
                Console.WriteLine((int)MenuOption.ProcessMarkovModel + ". Process Markov model");
                Console.WriteLine((int)MenuOption.WriteAggregateToFile + ". Write aggregate results to CSV");
                Console.WriteLine((int)MenuOption.WriteTransitionsToFile + ". Write transition results to CSV");
                Console.WriteLine((int)MenuOption.WriteDataToCsv + ". Write data back to CSV");
                Console.WriteLine((int)MenuOption.Exit + ". Exit");
                Console.Write(">> ");
                string rawInput = Console.ReadLine();
                if (Int32.TryParse(rawInput, out userChoice) == false)
                {
                    Console.WriteLine("Invalid input.");
                }
                else
                {
                    MenuOption selection = (MenuOption)userChoice;
                    switch (selection)
                    {
                        case MenuOption.ChangeDirectory:
                            Console.WriteLine("Feature not implemented.");
                            break;
                        case MenuOption.ListFiles:
                            Console.WriteLine("Feature not implemented.");
                            break;
                        case MenuOption.LoadFile:
                            LoadFile();
                            Console.WriteLine("File load complete.");
                            break;
                        case MenuOption.AppendFile:
                            AppendFile();
                            Console.WriteLine("File appended.");
                            break;
                        case MenuOption.LoadGrades:
                            Vm.AttachGrades();
                            Console.WriteLine("Grades attached.");
                            break;
                        case MenuOption.FilterGrades:
                            FilterStudentsByGrade(Vm);
                            break;
                        case MenuOption.NormalizeProgrammingStates:
                            Vm.NormalizeProgrammingStates();
                            Console.WriteLine("States normalized.");
                            break;
                        case MenuOption.CountTransitions:
                            Vm.ProcessTransitions();
                            Console.WriteLine("Transitions counted.");
                            break;
                        case MenuOption.ConstructMarkovModel:
                            ConstructMarkovModel(Vm);
                            break;
                        case MenuOption.ProcessMarkovModel:
                            ProcessMarkovModel(Vm);
                            break;
                        case MenuOption.WriteAggregateToFile:
                            Console.Write("Enter destination file: ");
                            rawInput = Console.ReadLine();
                            Vm.WriteTimeInStateToCsv(rawInput);
                            break;
                        case MenuOption.WriteTransitionsToFile:
                            Console.Write("Enter destination file: ");
                            rawInput = Console.ReadLine();
                            Vm.WriteTransitionsToCsv(rawInput);
                            break;
                        case MenuOption.WriteDataToCsv:
                            Console.Write("Enter destination file: ");
                            rawInput = Console.ReadLine();
                            Vm.WriteLoadedDataToCsv(rawInput);
                            break;
                        case MenuOption.Exit:
                            Console.WriteLine("Returning to main menu.");
                            break;
                        default:
                            break;
                    }
                }
                Console.WriteLine("");
            }
        }
    }
}

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
        private enum MenuOption { 
            ChangeDirectory = 1, 
            ListFiles, 
            LoadFile, 
            AppendFile, 
            LoadGrades, 
            NormalizeProgrammingStates, 
            CountTransitions, 
            WriteAggregateToFile, 
            WriteTransitionsToFile,
            Exit 
        }

        private TimelineAnalysisViewModel Vm { get; set; }

        public TimelineAnalysisView()
        {
            Vm = new TimelineAnalysisViewModel();
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
            if(file.Length > 0)
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
                Console.WriteLine((int)MenuOption.NormalizeProgrammingStates + ". Normalize programming states");
                Console.WriteLine((int)MenuOption.CountTransitions + ". Count transitions");
                Console.WriteLine((int)MenuOption.WriteAggregateToFile + ". Write aggregate results to CSV");
                Console.WriteLine((int)MenuOption.WriteTransitionsToFile + ". Write transition results to CSV");
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
                        case MenuOption.NormalizeProgrammingStates:
                            Vm.NormalizeProgrammingStates();
                            Console.WriteLine("States normalized.");
                            break;
                        case MenuOption.CountTransitions:
                            Vm.ProcessTransitions();
                            Console.WriteLine("Transitions counted.");
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

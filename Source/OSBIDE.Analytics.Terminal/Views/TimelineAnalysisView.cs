using OSBIDE.Analytics.Terminal.Models;
using OSBIDE.Analytics.Terminal.ViewModels;
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
        private enum MenuOption { ChangeDirectory = 1, ListFiles, AnalyzeFile, Exit }
      
        public void Run()
        {
            int userChoice = 0;
            while (userChoice != (int)MenuOption.Exit)
            {
                TimelineAnalysisViewModel vm = new TimelineAnalysisViewModel();

                Console.WriteLine((int)MenuOption.ChangeDirectory + ". Change working directory");
                Console.WriteLine((int)MenuOption.ListFiles + ". List all files in current directory");
                Console.WriteLine((int)MenuOption.AnalyzeFile + ". Analyze file");
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
                        case MenuOption.AnalyzeFile:
                            int counter = 1;

                            //enumerate files in working directory
                            List<string> files = Directory.EnumerateFiles(".", "*.csv").ToList();
                            foreach(string file in files)
                            {
                                Console.WriteLine("{0}. {1}", counter, file);
                                counter++;
                            }
                            Console.Write(">> ");
                            int fileToRead = -1;
                            string user_input = Console.ReadLine();
                            Int32.TryParse(user_input, out fileToRead);

                            //ensure valid selection
                            if(fileToRead > 0 && fileToRead < counter)
                            {
                                Dictionary<int, List<TimelineState>> states = vm.ParseTimeline(files[fileToRead - 1]);
                            }

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

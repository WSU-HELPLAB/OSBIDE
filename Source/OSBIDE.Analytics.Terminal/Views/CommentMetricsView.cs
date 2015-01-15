using OSBIDE.Analytics.Terminal.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Terminal.Views
{
    class CommentMetricsView
    {
        private enum MenuOption { LoadFeedPosts = 1, LoadLogComments, LoadSyllables, CalculateMetrics, OrganizeIntoThreads, WriteToCsv, Exit }
        public void Run()
        {
            int userChoice = 0;
            CommentMetricsViewModel vm = new CommentMetricsViewModel();
            while (userChoice != (int)MenuOption.Exit)
            {
                Console.WriteLine((int)MenuOption.LoadFeedPosts + ". Load feed posts");
                Console.WriteLine((int)MenuOption.LoadLogComments + ". Load log comments");
                Console.WriteLine((int)MenuOption.LoadSyllables + ". Load syllables");
                Console.WriteLine((int)MenuOption.CalculateMetrics + ". Perform metric calculations");
                Console.WriteLine((int)MenuOption.OrganizeIntoThreads + ". Organize data into threaded conversation");
                Console.WriteLine((int)MenuOption.WriteToCsv + ". Write results to CSV");
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
                        case MenuOption.LoadFeedPosts:
                            vm.LoadFeedPosts();
                            Console.WriteLine("Feed posts loaded...");
                            break;
                        case MenuOption.LoadLogComments:
                            vm.LoadLogComments();
                            Console.WriteLine("Log comments loaded...");
                            break;
                        case MenuOption.LoadSyllables:
                            vm.LoadSyllables();
                            Console.WriteLine("Syllables loaded...");
                            break;
                        case MenuOption.CalculateMetrics:
                            vm.AnalyzeComments();
                            Console.WriteLine("Calcluation complete...");
                            break;
                        case MenuOption.OrganizeIntoThreads:
                            vm.OrganizeComments();
                            Console.WriteLine("Comments organized into threads...");
                            break;
                        case MenuOption.WriteToCsv:
                            Console.Write("Enter destination file: ");
                            rawInput = Console.ReadLine();
                            vm.WriteToCsv(rawInput);
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

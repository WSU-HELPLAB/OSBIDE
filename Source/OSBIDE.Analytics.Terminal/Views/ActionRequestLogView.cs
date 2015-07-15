using OSBIDE.Analytics.Terminal.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Terminal.Views
{
    public class ActionRequestLogView
    {

        private enum MenuOption { LoadAccessLogs = 1, AggregateByWeek, GenerateWeeklyStatistics, Exit }

        public void Run()
        {
            int userChoice = 0;
            ActionRequestLogViewModel vm = new ActionRequestLogViewModel();
            while (userChoice != (int)MenuOption.Exit)
            {
                Console.WriteLine((int)MenuOption.LoadAccessLogs + ". Reload access logs from DB (costly)");
                Console.WriteLine((int)MenuOption.AggregateByWeek + ". Aggregate daily activity by week");
                Console.WriteLine((int)MenuOption.GenerateWeeklyStatistics + ". Generate summary weekly statistics by controller and action");
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
                        case MenuOption.LoadAccessLogs:
                            vm.LoadAllLogs();
                            Console.WriteLine("access logs parsed...");
                            break;
                        case MenuOption.AggregateByWeek:
                            List<List<string>> spreadsheet = vm.AggregateLogsByWeek();
                            Console.Write("Enter destination file: ");
                            string fileName = Console.ReadLine();
                            vm.WriteToCsv(spreadsheet, fileName);
                            Console.WriteLine("daily activity aggregated...");
                            break;
                        case MenuOption.GenerateWeeklyStatistics:

                            //aggreate weekly statistics
                            vm.AggregateLogsByWeek();

                            //generate one CSV file for each controller / action pairing
                            foreach(string controllerName in vm.ControllerActions.Keys)
                            {
                                foreach(string actionName in vm.ControllerActions[controllerName].Keys)
                                {
                                    List<List<string>> matrix = vm.FilterActivity(controllerName, actionName);
                                    string name = string.Format("weekly_{0}_{1}.csv", controllerName, actionName);
                                    vm.WriteToCsv(matrix, name);
                                }
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

using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSBIDE.Analytics.Terminal.Views;

namespace OSBIDE.Analytics.Terminal
{
    class Program
    {
        private enum MenuOption { CommentMetrics = 1, Exit }
        static void Main(string[] args)
        {

            int userChoice = 0;
            while (userChoice != (int)MenuOption.Exit)
            {
                Console.WriteLine("*** OSBIDE Analytics ***");
                Console.WriteLine((int)MenuOption.CommentMetrics + ". Run comment metrics");
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
                        case MenuOption.CommentMetrics:
                            CommentMetricsView view = new CommentMetricsView();
                            view.Run();
                            break;
                        case MenuOption.Exit:
                            Console.WriteLine("Exiting program.");
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}

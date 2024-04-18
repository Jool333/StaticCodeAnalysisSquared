using System.Reflection;
using GHAS;
using Sonar;
using StaticCodeAnalysisSquared.src.Entity;
using StaticCodeAnalysisSquared.src.FindGoodBad;

namespace StaticCodeAnalysisSquared.src.Main
{
    /// <summary>
    /// The main program class, runs the menu and calls all other runable classes.
    /// </summary>
    internal class Program
    {
        //Gets the project root directory.
        private static readonly string projectRoot = Directory.GetParent(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location))?.Parent?.Parent?.FullName;
        private static readonly string testCaseFolder = @"src/testcases";
        private static readonly string rulesFolder = Path.Combine(projectRoot, @"src/rules");

        private static readonly List<string> rulesTxtPaths = ["SemgrepRules.txt"/*, "SonarRules.txt", "GHASRules.txt"*/]; 
        private static readonly List<Option> options = [new(1, "Make workflow",false,false),
                                                        new(2, "Condense rules",false,false),
                                                        new(3, "Scan with SonarQube",false,false),
                                                        new(4, "Scan with Github Advanced Security",false,false),
                                                        new(5, "Scan with Snyk",false,false),
                                                        new(6, "Scan with SemGrep",false,false)];
        /// <summary>
        /// Starts the program, starts by reseting the optionslist, then starts the menu, the executes if there are any options selected.
        /// Once all options are executed it the ask if you want to go again and will start over if you do.
        /// </summary>
        /// <returns></returns>
        static async Task Main()
        {
            bool again;
            do
            {
                Reset();

                OperateMenu();

                await ExcecuteChoice();

                again = Again();

            } while (again);
            
        }
        /// <summary>
        /// Prints the menu based on the <see cref="options"/> list, starts by clearing the console.
        /// A currently selected option is made green while the rest iw white.
        /// </summary>
        private static void PrintMenu()
        {
            ClearConsole();
            
            Console.WriteLine(
                "Static Code Analysis^2 started!\n" +
                "Select processes to run:\nPress space to select and enter to run.\nShift+Enter to run all processes.\n" +
                "Ctrl+Shift+Enter to run all scanners.\nPress Esc to exit the program.");
            foreach (var option in options)
            {
                if (option.IsCurrentlySelected)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                if (option.IsSelected)
                {
                    Console.Write("\t [x]\t");
                }
                else
                {
                    Console.Write("\t [ ]\t");
                }
                Console.Write(option.Message + "\n");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        /// <summary>
        /// A console clearing method that overwrites the existing text, produces much less flashing compared to <see cref="Console.Clear"/>.
        /// </summary>
        public static void ClearConsole()
        {
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;
            for (int i = 0; i < Console.WindowHeight; i++)
                Console.Write(new String(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = true;
        }
        /// <summary>
        /// Reads key input and updated the options accordingly, then either prints the menu, starts the processes or exits the program. 
        /// </summary>
        private static void OperateMenu()
        {
            bool start = false;
            int index = 0;
            ConsoleKeyInfo keyInfo;
            do
            {
                Console.CursorVisible = false;
                PrintMenu();
                keyInfo = Console.ReadKey();

                if (keyInfo.Key == ConsoleKey.DownArrow && index + 1 < options.Count)
                {
                    options.Where(x => x.IsCurrentlySelected = true).ToList().ForEach(x => x.IsCurrentlySelected = false);
                    index++;
                    options[index].IsCurrentlySelected = true;
                }
                if (keyInfo.Key == ConsoleKey.UpArrow && index - 1 >= 0)
                {
                    options.Where(x => x.IsCurrentlySelected = true).ToList().ForEach(x => x.IsCurrentlySelected = false);
                    index--;
                    options[index].IsCurrentlySelected = true;
                }
                if (keyInfo.Key == ConsoleKey.Spacebar)
                {
                    options[index].IsSelected = !options[index].IsSelected;
                }
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    if (keyInfo.Modifiers == ConsoleModifiers.Shift)
                    {
                        options.ForEach(x => x.IsSelected = true);
                    }
                    if ((keyInfo.Modifiers & (ConsoleModifiers.Shift | ConsoleModifiers.Control)) == (ConsoleModifiers.Shift | ConsoleModifiers.Control))
                    {
                        options.Where(x=>x.Message.Contains("Scan")).ToList().ForEach(x => x.IsSelected = true);
                    }
                    if(options.Any(x => x.IsSelected == true))
                    {
                        start = true;
                    }
                }
                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Exit();
                }
                PrintMenu();

            } while (!start);

            ClearConsole();
        }
        /// <summary>
        /// Runs the processes depending on the selected options, only loads goodbad data if it performing a sca tool process.
        /// Each process is divided by a screen wide line.
        /// </summary>
        /// <returns></returns>
        private static async Task ExcecuteChoice()
        {
            string testcasePath = Path.Combine(projectRoot, testCaseFolder);
            List<GoodBadEntity> allGoodBad = [];

            // only load goodbad if performing relevant task
            if (options.Where(x => x.Id > 2).Select(x => x.IsSelected).Any(x => x == true))
            {
                allGoodBad = FindGoodBadCases.FindInDirectory(testcasePath);
                await Console.Out.WriteLineAsync(allGoodBad.Count.ToString()+"\n");
            }


            foreach (var option in options.Where(x => x.IsSelected == true))
            {
                switch (option.Id)
                {
                    case 1:
                        PrintLine();
                        Workflow.MakeWorkflow(testcasePath);
                        break;
                    case 2:
                        PrintLine();
                        foreach (var ruleTxt in rulesTxtPaths)
                        {
                            RuleCondenser.Condense(Path.Combine(Path.Combine(projectRoot, rulesFolder), ruleTxt));
                        }
                        break;
                    case 3:
                        PrintLine();
                        string sonarResult = await SonarScraper.GetSonarResults(allGoodBad);
                        await Console.Out.WriteLineAsync(sonarResult);
                        break;
                    case 4:
                        PrintLine();
                        string ghasResult = await GHApiScraper.GetGhasResults(allGoodBad);
                        await Console.Out.WriteLineAsync(ghasResult);
                        break;
                    case 5:
                        PrintLine();
                        string snyk = "SnykScan";
                        string snykResult = await GHApiScraper.GetGhasResults(allGoodBad, snyk);
                        await Console.Out.WriteLineAsync(snykResult);
                        break;
                    case 6:
                        PrintLine();
                        string sem1 = "SemgrepScan1";
                        string sem1Result = await GHApiScraper.GetGhasResults(allGoodBad, sem1);
                        await Console.Out.WriteLineAsync(sem1Result);

                        string sem2 = "SemgrepScan2";
                        string sem2Result = await GHApiScraper.GetGhasResults(allGoodBad, sem2);
                        await Console.Out.WriteLineAsync(sem2Result);
                        break;
                    case 0:
                        break;
                }
            }
            PrintLine();
        }
        /// <summary>
        /// Sees if the user wants to run the program again, only accepts y/Y or n/N as input.
        /// if y it restarts, n it exits.
        /// </summary>
        private static bool Again()
        {
            Console.CursorVisible = true;
            Console.Write("\nDo you want to run more tests? y/n:\t");
            ConsoleKeyInfo readKey;
            bool again = false;
            bool valid = false;
            do
            {
                readKey = Console.ReadKey();
                if(readKey.Key == ConsoleKey.N || readKey.Key == ConsoleKey.Y)
                {
                    valid = true;
                    if (readKey.Key == ConsoleKey.Y)
                    {
                        again = true;
                    }
                }
                else
                {
                    Console.Write("\b \b");
                }
                
            } while (!valid);
            
            return again;
        }
        /// <summary>
        /// Sees if the user wants to exit the program, only accepts y/Y or n/N as input.
        /// if n it continues where it was, y it exits.
        /// </summary>
        private static void Exit()
        {
            Console.CursorVisible = true;
            Console.Write("\nAre you sure you want to exit? y/n:\t");
            ConsoleKeyInfo readKey;
            bool valid = false;
            do
            {
                readKey = Console.ReadKey();
                if (readKey.Key == ConsoleKey.N || readKey.Key == ConsoleKey.Y)
                {
                    valid = true;
                    if (readKey.Key == ConsoleKey.Y)
                    {
                        Environment.Exit(0);
                    }
                }
                else
                {
                    Console.Write("\b \b");
                }

            } while (!valid);
        }

        /// <summary>
        /// Resets the options list to its initial form.
        /// </summary>
        private static void Reset()
        {
            foreach (var option in options)
            {
                option.IsSelected = false;
                option.IsCurrentlySelected = false;
            }

            options[0].IsCurrentlySelected = true;
        }
        /// <summary>
        /// prints a window wide line.
        /// </summary>
        private static void PrintLine()
        {
            for (int i = 0; i < Console.WindowWidth; i++)
            {
                Console.Write("─");
            }
            Console.Write("\n");
        }
    }

    /// <summary>
    /// Class for each menu option and its parameters
    /// </summary>
    /// <param name="id"></param>
    /// <param name="message"></param>
    /// <param name="isSelected"></param>
    /// <param name="isCurrentlySelected"></param>
    public class Option(int id, string message, bool isSelected, bool isCurrentlySelected)
    {
        public int Id { get; set; } = id;
        public string Message { get; set; } = message;
        public bool IsSelected { get; set; } = isSelected;
        public bool IsCurrentlySelected { get; set; } = isCurrentlySelected;
    }
}

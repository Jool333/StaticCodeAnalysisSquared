using GHAS;
using Sonar;
using StaticCodeAnalysisSquared.src.Entity;
using StaticCodeAnalysisSquared.src.FindGoodBad;

namespace StaticCodeAnalysisSquared.src.Main
{
    internal class Program
    {
        private static readonly string directoryPath = "C:\\Users\\johanols\\Desktop\\"; // replace with path to juliet test cases and any rules txts
        private static readonly string testCaseFolder = "TestCaseCollection"; // replace with the folder containing the individual testcases
        private static readonly List<string> rulesTxtPaths = ["SemgrepRules.txt"/*, "SonarRules.txt", "GHASRules.txt"*/]; 
        private static readonly List<Option> options = [new(1, "Make workflow",false,false),
                                                        new(2, "Condense rules",false,false),
                                                        new(3, "Scan with SonarQube",false,false),
                                                        new(4, "Scan with Github Advanced Security",false,false),
                                                        new(5, "Scan with Snyk",false,false),
                                                        new(6, "Scan with SemGrep",false,false)];
        static async Task Main()
        {
            bool again;
            do
            {
                Reset();

                options[0].IsCurrentlySelected = true;

                PrintMenu();

                OperateMenu();

                ClearConsole();

                await ExcecuteChoice();

                again = Again();

            } while (again);
            
        }
        private static void PrintMenu()
        {
            ClearConsole();
            Console.WriteLine(
                "Static Code Analysis^2 started!\n" +
                "Select processes to run, press space to select and enter to run. Shift+Enter to run all, Ctrl+Shift+Enter to run all scanners");
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

        public static void ClearConsole()
        {
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;
            for (int i = 0; i < Console.WindowHeight; i++)
                Console.Write(new String(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = true;
        }
        private static void OperateMenu()
        {
            bool start = false;
            int index = 0;
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey();

                if (keyInfo.Key == ConsoleKey.DownArrow && index + 1 < options.Count)
                {
                    options.Where(x => x.IsCurrentlySelected = true).ToList().ForEach(x => x.IsCurrentlySelected = false);
                    index++;
                    options[index].IsCurrentlySelected = true;
                    PrintMenu();
                }
                if (keyInfo.Key == ConsoleKey.UpArrow && index - 1 >= 0)
                {
                    options.Where(x => x.IsCurrentlySelected = true).ToList().ForEach(x => x.IsCurrentlySelected = false);
                    index--;
                    options[index].IsCurrentlySelected = true;
                    PrintMenu();
                }
                if (keyInfo.Key == ConsoleKey.Spacebar)
                {
                    options[index].IsSelected = !options[index].IsSelected;
                    PrintMenu();
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
                PrintMenu();

            } while (!start);
        }
        private static async Task ExcecuteChoice()
        {
            string testcasePath = Path.Combine(directoryPath, testCaseFolder);
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
                        Workflow.MakeWorkflow(testcasePath);
                        break;
                    case 2:
                        foreach (var ruleTxt in rulesTxtPaths)
                        {
                            RuleCondenser.Condense(Path.Combine(directoryPath, ruleTxt));
                        }
                        break;
                    case 3:
                        string sonarResult = await SonarScraper.GetSonarResults(allGoodBad);
                        await Console.Out.WriteLineAsync(sonarResult);
                        break;
                    case 4:
                        string ghasResult = await GhasScraper.GetGhasResults(allGoodBad);
                        await Console.Out.WriteLineAsync(ghasResult);
                        break;
                    case 5:
                        string snyk = "SnykScan";
                        string snykResult = await GhasScraper.GetGhasResults(allGoodBad, snyk);
                        await Console.Out.WriteLineAsync(snykResult);
                        break;
                    case 6:
                        string sem1 = "SemgrepScan1";
                        string sem1Result = await GhasScraper.GetGhasResults(allGoodBad, sem1);
                        await Console.Out.WriteLineAsync(sem1Result);

                        string sem2 = "SemgrepScan2";
                        string sem2Result = await GhasScraper.GetGhasResults(allGoodBad, sem2);
                        await Console.Out.WriteLineAsync(sem2Result);
                        break;
                    case 0:
                        break;
                }
            }
            

        }
        private static bool Again()
        {
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

        private static void Reset()
        {
            foreach (var option in options)
            {
                option.IsSelected = false;
                option.IsCurrentlySelected = false;
            }
        }
    }
    public class Option(int id, string message, bool isSelected, bool isCurrentlySelected)
    {
        public int Id { get; set; } = id;
        public string Message { get; set; } = message;
        public bool IsSelected { get; set; } = isSelected;
        public bool IsCurrentlySelected { get; set; } = isCurrentlySelected;
    }
}

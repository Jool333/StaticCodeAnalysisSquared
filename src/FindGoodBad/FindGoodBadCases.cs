using StaticCodeAnalysisSquared.src.Entity;

namespace StaticCodeAnalysisSquared.src.FindGoodBad
{
    /// <summary>
    /// Class for finding good and bad entities in a given directory.
    /// </summary>
    public class FindGoodBadCases
    {
        private const string txtFilePath = "C:\\Users\\johanols\\Desktop\\BadGoodData.txt"; // file to save data in for comparison
        private static readonly List<GoodBadEntity> goodBadList = [];
        private static string allData = "";
        private static int allGoodCount = 0;
        private static int allBadCount = 0;

        /// <summary>
        /// Finds all files in a directory recursivly. Then prints the result in a file with the path <see cref="txtFilePath"/>.
        /// Also prints amount of entites found in console.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public static List<GoodBadEntity> FindInDirectory(string directoryPath)
        {
            Console.WriteLine("Loading GoodBad");
            RecursiveDirectoryJumping(directoryPath);
            PrintResults(allData);
            Console.WriteLine("Done getting GoodBad\n" +
                $"Nbr of Bad cases: {allBadCount}\n" +
                $"Nbr of Good cases: {allGoodCount}\n" +
                $"Total cases: {allGoodCount + allBadCount}");

            return goodBadList;
        }
        /// <summary>
        /// Jumps recursivly until it finds a directory with files in, then adds data.
        /// </summary>
        /// <param name="directoryPath"></param>
        public static void RecursiveDirectoryJumping(string directoryPath)
        {
            if (Directory.GetFiles(directoryPath).Length == 0)
            {
                foreach (var folder in Directory.GetDirectories(directoryPath))
                {
                    RecursiveDirectoryJumping(folder);
                }
            }
            else
            {
                AddData(directoryPath);
            }
        }
        /// <summary>
        /// Adds all data if the file is a class file, not a project file, doesnt contain the "Program" or "_base".
        /// </summary>
        /// <param name="directoryPath"></param>
        public static void AddData(string directoryPath)
        {
            foreach (var filePath in Directory.GetFiles(directoryPath)
                .Where(x => x.Contains(".cs") && !x.Contains(".csproj") && !x.Contains("Program") && !x.Contains("_base")))
            {
                int lastSlash = filePath.Contains('\\') ? filePath.LastIndexOf('\\') : 0;
                string croppedFilePath = filePath[(lastSlash + 1)..];

                string final = croppedFilePath + " " + FindBadAndGoodBlocksInFile(filePath) + "\n";
                allData += final;
            }
        }
        /// <summary>
        /// Finds good and bad entires in a file important to change the int in the split('\\')[change here] when making the category variable so that it matches the 
        /// part of the path that is the category for example:
        /// "C:\Users\johanols\Desktop\TestCaseCollection\testcases\CWE80_XSS\s02\CWE80_XSS__Web_QueryString_Web_01.cs" you want the 7th part thus the number is 6
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string FindBadAndGoodBlocksInFile(string filePath)
        {
            int lastSlash = filePath.Contains('\\') ? filePath.LastIndexOf('\\') : 0;
            string croppedFilePath = filePath[(lastSlash + 1)..];
            string category = filePath.Split('\\')[6]; // what ever the main folder for the test cases is 

            List<BadEntity> badList = FindBad.FindBadInFile(filePath);
            allBadCount += badList.Count;

            List<GoodEntity> goodList = FindGood.FindGoodInFile(filePath);
            allGoodCount += goodList.Count;

            goodBadList.Add(new GoodBadEntity() { Category = category, Name = croppedFilePath, Bad = badList, Good = goodList });

            string returnString;

            if (goodList.Count != 0 && badList.Count != 0)
            {
                returnString = $"\nBad start: {badList[0].Start} End: {badList[^1].End}" +
                $"\nGood Start: {goodList[0].Start} End: {goodList[^1].End}";
            }
            else if (goodList.Count != 0 && badList.Count == 0)
            {
                returnString = $"\nGood Start: {goodList[0].Start} End: {goodList[^1].End}";
            }
            else
            {

                returnString = $"\nBad start: {badList[0].Start} End: {badList[^1].End}";
            }

            return returnString;
        }

        public static void PrintResults(string data)
        {
            File.WriteAllText(txtFilePath, data);
        }
    }
}

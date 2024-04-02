using StaticCodeAnalysisSquared.src.Entity;

namespace StaticCodeAnalysisSquared.src.FindGoodBad
{
    public class FindGoodBadCases
    {
        private const string txtFilePath = "C:\\Users\\johanols\\Desktop\\BadGoodData.txt"; // file to save data in for comparison
        private static readonly List<GoodBadEntity> goodBadList = [];
        private static string allData = "";
        private static int allGoodCount = 0;
        private static int allBadCount = 0;

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
        public static string FindBadAndGoodBlocksInFile(string filePath)
        {
            int lastSlash = filePath.Contains('\\') ? filePath.LastIndexOf('\\') : 0;
            string croppedFilePath = filePath[(lastSlash + 1)..];
            string category = filePath.Split('\\')[6]; // what ever the main folder for the test cases is 

            List<BadEntity> badList = FindBad.FindBadInDiretory(filePath);
            allBadCount += badList.Count;

            List<GoodEntity> goodList = FindGood.FindGoodBlocksInFile(filePath);
            allGoodCount += goodList.Count;

            goodBadList.Add(new GoodBadEntity() { Category = category, Name = croppedFilePath, Bad = badList, Good = goodList });

            string returnString;

            if ( goodList.Count != 0 && badList.Count != 0 )
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
            

            //testcases\CWE80_XSS\s02\CWE80_XSS__Web_QueryString_Web_01.cs
            
            return returnString;
        }

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

        public static void PrintResults(string data)
        {
            File.WriteAllText(txtFilePath, data);
        }
    }
}

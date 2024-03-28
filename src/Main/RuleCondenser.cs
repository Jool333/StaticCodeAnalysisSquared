namespace StaticCodeAnalysisSquared.src.Main
{
    internal class RuleCondenser
    {
        public static void Condense(string filePath)
        {
            List<string> wholeFile = File.ReadLines(filePath).Select(x=>x.Split(" ")[0]).ToList();

            wholeFile.Sort();

            foreach (var line in wholeFile.Distinct()) 
            {
                Console.WriteLine(line);
            }
        }
    }
}

namespace StaticCodeAnalysisSquared.src.Main
{
    /// <summary>
    /// Class used to condense CWE rules for any tool.
    /// </summary>
    internal class RuleCondenser
    {
        /// <summary>
        /// Reads all lines from a txt file and takes the first part before a whitespace that is unique into a list.
        /// Sorts the list and and then prints it.
        /// </summary>
        /// <param name="filePath"></param>
        public static void Condense(string filePath)
        {
            List<string> wholeFile = File.ReadLines(filePath).Select(x=>x.Split(" ")[0]).Distinct().ToList();

            wholeFile.Sort();

            foreach (var line in wholeFile) 
            {
                Console.WriteLine(line);
            }
        }
    }
}

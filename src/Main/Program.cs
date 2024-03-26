using GHAS;
using Sonar;
using StaticCodeAnalysisSquared.src.FindGoodBad;

namespace StaticCodeAnalysisSquared.src.Main
{
    internal class Program
    {
        private static readonly string directoryPath = "C:\\Users\\johanols\\Desktop\\TestCaseCollection"; // replace with path to juliet test cases
        static async Task Main()
        {
            // Workflow.MakeWorkflow(directoryPath);

            var allGoodBad = FindGoodBadCases.FindInDirectory(directoryPath);
            await Console.Out.WriteLineAsync(allGoodBad.Count.ToString());
 
            string sonarResult = await SonarScraper.GetSonarResults(allGoodBad);
            await Console.Out.WriteLineAsync(sonarResult);

            string ghasResult = await GhasScraper.GetGhasResults(allGoodBad);
            await Console.Out.WriteLineAsync(ghasResult);

            string snyk = "SnykScan";

            string snykResult = await GhasScraper.GetGhasResults(allGoodBad, snyk);
            await Console.Out.WriteLineAsync(snykResult);

            string sem1 = "SemgrepScan1";
            string sem1Result = await GhasScraper.GetGhasResults(allGoodBad, sem1);
            await Console.Out.WriteLineAsync(sem1Result);

            string sem2 = "SemgrepScan2";
            string sem2Result = await GhasScraper.GetGhasResults(allGoodBad, sem2);
            await Console.Out.WriteLineAsync(sem2Result);
        }
    }
}

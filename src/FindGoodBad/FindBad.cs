using StaticCodeAnalysisSquared.src.Entity;

namespace StaticCodeAnalysisSquared.src.FindGoodBad
{
    /// <summary>
    /// Class for finding all bad entities.
    /// </summary>
    internal class FindBad
    {
        /// <summary>
        /// Finds all bad entities in a given <paramref name="filePath"/> and returns them in a list.
        /// </summary>
        /// <param name="filePath"></param>
        public static List<BadEntity> FindBadInFile(string filePath)
        {
            var wholeFile = File.ReadLines(filePath);
            int count = 1;

            List<BadEntity> badList = [];
            List<int> badLines = [];

            bool foundEnd = false;

            foreach (var row in wholeFile)
            {
                if (filePath.Contains("bad"))
                {
                    switch (row)
                    {
                        case "#if (!OMITBAD)":
                            badLines.Add(count); 
                            break;
                        case "#endif //omitbad":
                            badLines.Add(count);
                            foundEnd = true;
                            break;
                        case "#endif":
                            badLines.Add(count);
                            foundEnd = true;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    if (row.Contains("void Bad") || row.Contains("void BadS"))
                    {
                        badLines.Add(count);
                    }
                    switch (row)
                    {
                        case "#endif //omitbad":
                            badLines.Add(count);
                            foundEnd = true;
                            break;
                        case "#endif":
                            badLines.Add(count);
                            foundEnd = true;
                            break;
                        default:
                            break;
                    }
                }
                if (foundEnd)
                {
                    break;
                }
                
                count++;
            }

            for (int i = 0; i < badLines.Count - 1; i++)
            {
                BadEntity bad = new()
                {
                    Start = badLines[i],
                    End = badLines[i + 1] - 1
                };
                badList.Add(bad);
            }
            return badList;
        }
    }
}
using StaticCodeAnalysisSquared.src.Entity;

namespace StaticCodeAnalysisSquared.src.FindGoodBad
{
    internal class FindBad
    {
        public static List<BadEntity> FindBadInDiretory(string filePath)
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
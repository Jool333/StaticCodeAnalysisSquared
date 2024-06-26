﻿using StaticCodeAnalysisSquared.src.Entity;

namespace StaticCodeAnalysisSquared.src.FindGoodBad
{
    /// <summary>
    /// Class for finding all bad entities.
    /// </summary>
    internal class FindGood
    {
        /// <summary>
        /// Finds all good entities in a given <paramref name="filePath"/> and returns them in a list.
        /// </summary>
        /// <param name="filePath"></param>
        public static List<GoodEntity> FindGoodInFile(string filePath)
        {
            var wholeFile = File.ReadLines(filePath);
            int count = 1;

            List<GoodEntity> goodList = [];
            List<int> goodLines = [];

            foreach (var row in wholeFile)
            {
                if (filePath.Contains("good"))
                {
                    switch (row)
                    {
                        case "#if (!OMITGOOD)":
                            goodLines.Add(count); 
                            break;
                        case "#endif //omitgood":
                            goodLines.Add(count);
                            break;
                        case "#endif":
                            goodLines.Add(count);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    if (row.Contains("private void Good") || row.Contains("static void Good"))
                    {
                        goodLines.Add(count);
                    }
                    switch (row)
                    {
                        case "#endif //omitgood":
                            goodLines.Add(count);
                            break;
                        case "#endif":
                            goodLines.Add(count);
                            break;
                        default:
                            break;
                    }
                }
                count++;
            }

            for (int i = 0; i < goodLines.Count-1; i++)
            {
                GoodEntity good = new()
                {
                    Start = goodLines[i],
                    End = goodLines[i + 1]-1
                };
                goodList.Add(good);
            }
            return goodList;
        }
    }
}

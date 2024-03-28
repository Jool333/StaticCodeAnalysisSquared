using System.Text.Json;
using StaticCodeAnalysisSquared.src.Entity;
using StaticCodeAnalysisSquared.src.Scrapers.Secrets;

namespace Sonar
{
    public class SonarScraper
    {
        private static readonly SecretData secrets = SecretData.GetAllData();
        private static readonly string token = secrets.SonarKey;
        private static readonly JsonSerializerOptions jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public static async Task<string> GetSonarResults(List<GoodBadEntity> allGoodBad)
        {
            string resultString = $"SonarQube results are:\n";
            int truePositive = 0;
            int falsePositive = 0;
            int falseNegative = 0;
            int trueNegative = 0;
            int duplicate = 0;

            List<CategoryResults> categoriesData = CreateCategoryResultList(allGoodBad);

            List<Hotspot> allHotspots = await GetAllHotspots();

            PrintHotspotCategories(allHotspots);

            await Console.Out.WriteLineAsync("Analysing SonarQube results");

            foreach (var file in allGoodBad)
            {
                var matchedScrapedResults = allHotspots.Where(x => x.Component.Contains(file.Name)).ToList();
                var lines = matchedScrapedResults.Select(x => x.Line).Distinct().ToList();

                List<GoodEntity> goodList = [.. file.Good];

                List<BadEntity> badList = [.. file.Bad];

                bool foundMatch = false;

                if (lines.Count != 0)
                {
                    foreach (var line in lines)
                    {
                        if (badList.Count != 0)
                        {
                            foreach (var bad in badList)
                            {
                                if (line <= bad.End && line >= bad.Start)
                                {
                                    categoriesData.Where(x => x.Category == file.Category && x.ResultType == "tp").ToList().ForEach(x => x.Counter++);
                                    truePositive++;
                                    badList.Remove(bad);
                                    foundMatch = true;
                                    break;
                                }
                            }
                            if (foundMatch)
                            {
                                foundMatch = false;
                                continue;
                            }
                        }
                        if (goodList.Count != 0)
                        {
                            foreach (var good in goodList)
                            {
                                if (line <= good.End && line >= good.Start)
                                {
                                    categoriesData.Where(x => x.Category == file.Category && x.ResultType == "fp").ToList().ForEach(x => x.Counter++);
                                    falsePositive++;
                                    goodList.Remove(good);
                                    foundMatch = true;
                                    break;
                                }
                            }
                            if (foundMatch)
                            {
                                foundMatch = false;
                                continue;
                            }
                        }
                        duplicate++;
                        //await Console.Out.WriteLineAsync($"Dupe: {file.Name}\t{line}");
                    }
                    categoriesData.Where(x => x.Category == file.Category && x.ResultType == "fn").ToList().ForEach(x => x.Counter += badList.Count);
                    categoriesData.Where(x => x.Category == file.Category && x.ResultType == "tn").ToList().ForEach(x => x.Counter += goodList.Count);

                    trueNegative += goodList.Count;
                    falseNegative += badList.Count;
                }
                else
                {
                    categoriesData.Where(x => x.Category == file.Category && x.ResultType == "fn").ToList().ForEach(x => x.Counter += file.Bad.Count);
                    categoriesData.Where(x => x.Category == file.Category && x.ResultType == "tn").ToList().ForEach(x => x.Counter += file.Good.Count);

                    trueNegative += file.Good.Count;
                    falseNegative += file.Bad.Count;
                }
            }

            double precision = (double)(truePositive)/(double)(truePositive+falsePositive);
            double recall = (double)(truePositive)/ (double)(truePositive+falseNegative);
            double fScore = (2*precision*recall)/(precision+recall);
            double tp = truePositive;
            double fp = falsePositive;
            double tn = trueNegative;
            double fn = falseNegative;
            double mcc = (tp * tn - fp * fn) / Math.Sqrt((tp + fp) * (tp + fn) * (tn + fp) * (tn + fn));

            resultString += 
                $"True Positives:  {truePositive}\n" +
                $"False Positive:  {falsePositive}\n" +
                $"False Negative: {falseNegative}\n" +
                $"True Negative: {trueNegative}\n" +
                $"dupe/miss: {duplicate}\n" +
                $"{truePositive + falseNegative}\n" +
                $"precision: {precision}, recall: {recall} " +
                $"f-score: {fScore} MCC: {mcc}\n";

            return resultString;
        }
        private static async Task<List<Hotspot>> GetAllHotspots()
        {
            List<Hotspot> allHotspots = await ScrapeSonarAsync();
            List<Hotspot> validHotspots = allHotspots.Where(x => x.Component.Contains("CWE")).ToList();
            await Console.Out.WriteLineAsync(validHotspots.Count.ToString());
            return validHotspots;
        }

        private static async Task<List<Hotspot>> ScrapeSonarAsync(string project = "SelectedTests")
        {
            SecretData secrets = SecretData.GetAllData();
            string token = secrets.SonarKey;

            HttpClient client = new();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            client.BaseAddress = new Uri("http://localhost:9000/api/hotspots/search?");

            int page = 1;
            bool valid = true;

            List<Hotspot> fetchedHotspots = [];

            do
            {
                var response = await client.GetAsync($"inNewCodePeriod=false&onlyMine=false&p={page}&project={project}&ps=500&status=TO_REVIEW");

                if (response.IsSuccessStatusCode)
                {
                    await Console.Out.WriteLineAsync($"Loading SonarQube data page: {page}");
                    var json = response.Content.ReadAsStringAsync().Result;

                    var roots = JsonSerializer.Deserialize<Root>(json, jsonOptions);
                    fetchedHotspots.AddRange(collection: roots?.Hotspots);
                    
                    valid = roots.Paging.Total / roots.Paging.PageSize >= roots.Paging.PageIndex;
                    page++;
                }
                else
                {
                    valid = false;
                }

            } while (valid);

            if (fetchedHotspots.Count > 0)
            {
                await Console.Out.WriteLineAsync($"SonarQube data loaded for {project}");
            }
            else
            {
                await Console.Out.WriteLineAsync($"No data for {project}");
            }

            
            List<Hotspot> uniqueHotspots = fetchedHotspots.GroupBy(h => new { h.Component, h.Line })
                                       .Select(g => g.First())
                                       .ToList();

            List<string> filterList = new(); // { "Make sure using this hardcoded IP address '10.10.1.10' is safe here.", };
            if (filterList.Count > 0)
            {
               uniqueHotspots = FilterHotspots(uniqueHotspots, filterList);
            }

            return [.. uniqueHotspots.OrderBy(x => x.Component).ThenBy(x => x.Line)];
        }

        private static List<CategoryResults> CreateCategoryResultList(List<GoodBadEntity> goodBad)
        {
            List<string> allCategories = goodBad.Select(x => x.Category).Distinct().ToList();
            List<CategoryResults> categoriesData = [];

            foreach (var category in allCategories)
            {
                categoriesData.Add(new CategoryResults(category, "tp", 0));
                categoriesData.Add(new CategoryResults(category, "fp", 0));
                categoriesData.Add(new CategoryResults(category, "tn", 0));
                categoriesData.Add(new CategoryResults(category, "fn", 0));
            }

            return categoriesData;
        }

        private static void PrintHotspotCategories(List<Hotspot> hotspots)
        {
            Console.WriteLine("All found categories in hotspots:");
            List<string> hotspotCategories = hotspots.Select(x => x.Component.Split(":")[1].Split("/")[0] + ": " + x.SecurityCategory + ": " + x.Message).Distinct().ToList();

            foreach (var category in hotspotCategories)
            {
                Console.WriteLine(category);
            }
        }

        private static List<Hotspot> FilterHotspots(List<Hotspot> unfilteredHotspots, List<string> filterList)
        {
            List<Hotspot> filteredHotspots = [];

            foreach (var issue in filterList)
            {
                filteredHotspots = unfilteredHotspots.Where(x => x.Message != issue).ToList();
            }
            return [.. filteredHotspots.OrderBy(x => x.Component).ThenBy(x => x.Line)];
        }
    }
}

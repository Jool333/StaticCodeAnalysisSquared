using System.Net.Http.Headers;
using System.Text.Json;
using StaticCodeAnalysisSquared.src.Entity;
using StaticCodeAnalysisSquared.src.Scrapers.Secrets;

namespace GHAS
{
    public class GHApiScraper
    {
        private static readonly SecretData secrets = SecretData.GetAllData();
        private static readonly string token = secrets.GithubToken;
        public static readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allGoodBad"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public static async Task<string> GetGhasResults(List<GoodBadEntity> allGoodBad, string project = "TestCasesCurated")
        {
            string tool = project == "TestCasesCurated" ? "GHAS" : project == "SnykScan" ? "Snyk" : "Semgrep";
            string resultString = $"{tool} results are:\n";
            int truePositive = 0;
            int falsePositive = 0;
            int falseNegative = 0;
            int trueNegative = 0;
            int duplicate = 0;

            List<CategoryResults> categoriesData = CreateCategoryResultList(allGoodBad);

            List<GithubEntity> allHotspots = await GetValidHotspots(project);

            PrintHotspotCategories(allHotspots);

            await Console.Out.WriteLineAsync($"\nAnalysing {tool} results... \n");

            foreach (var file in allGoodBad)
            {
                var matchedScrapedResults = allHotspots.Where(x => x.Path.Contains(file.Name)).ToList();
                var lines = matchedScrapedResults.Select(x => x.Line).ToList();
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
                    categoriesData.Where(x => x.Category == file.Category && x.ResultType == "fn").ToList().ForEach(x => x.Counter += badList.Count);
                    falseNegative += badList.Count;
                    categoriesData.Where(x => x.Category == file.Category && x.ResultType == "tn").ToList().ForEach(x => x.Counter += goodList.Count);
                    trueNegative += goodList.Count;
                }

            }
            double precision = (double)(truePositive) / (double)(truePositive + falsePositive);
            double recall = (double)(truePositive) / (double)(truePositive + falseNegative);
            double fScore = (2 * precision * recall) / (precision + recall);
            double tp = truePositive;
            double fp = falsePositive;
            double tn = trueNegative;
            double fn = falseNegative;
            double mcc = (tp * tn - fp * fn)/Math.Sqrt( (tp + fp)*(tp + fn)*(tn+fp)*(tn+fn));

            resultString += 
                $"\nTrue Positives:  {truePositive}\n" +
                $"False Positive:  {falsePositive}\n" +
                $"False Negative: {falseNegative}\n" +
                $"True Negative: {trueNegative}\n" +
                $"Duplicates: {duplicate}\n" +
                $"Precision: {precision}, Recall: {recall} " +
                $"F-score: {fScore} MCC: {mcc}\n";

            return resultString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        private static async Task<List<GithubEntity>> GetValidHotspots(string project)
        {
            List<GithubEntity> allHotspots = await ScrapeGhasAsync(project);
            List<GithubEntity> validHotspots = allHotspots.Where(x => x.Path.Contains("CWE")).ToList();

            await Console.Out.WriteLineAsync($"Valid security hotspots: {validHotspots.Count}\n");

            return validHotspots;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        private static async Task<List<GithubEntity>> ScrapeGhasAsync(string project)
        {
            string? owner = project == "TestCasesCurated" ? secrets.GithubOwner1 : secrets.GithubOwner2 ?? throw new ArgumentNullException($"{secrets.GithubOwner2}","owner 2 not specified")
                            ;

            List<RootObject> fetchedHotspots = [];
            int page = 1;
            int itemsPerPage = 100;
            bool valid = true;

            HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.BaseAddress = new Uri("https://api.github.com/");
            client.DefaultRequestHeaders.UserAgent.TryParseAdd(secrets.GithubUser);

            // First call to find amount of alerts
            int nbrOfAlerts = await GetAlertAmountAsync(client, owner, project);
            await Console.Out.WriteLineAsync($"Amount of security alerts found: {nbrOfAlerts}");

            await Console.Out.WriteLineAsync($"Loading GHApi data");
            do
            {
                var response = await client.GetAsync($"repos/{owner}/{project}/code-scanning/alerts?page={page}&per_page={itemsPerPage}");
                await Console.Out.WriteAsync($"\rLoading GHApi data page: {page}");
                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;

                    var alertResults = JsonSerializer.Deserialize<List<RootObject>>(json, jsonSerializerOptions);
                    if (alertResults != null)
                    {
                        fetchedHotspots.AddRange(collection: alertResults);
                    }
                    valid = nbrOfAlerts / itemsPerPage >= page;
                    page++;
                }
                else
                {
                    valid = false;
                }

            } while (valid);

            await Console.Out.WriteLineAsync("\nAll pages loaded\n");
            var convertedHotspots = GithubEntity.Convert(fetchedHotspots);
            var uniqueHotspots = convertedHotspots.GroupBy(h => new { h.Path, h.Line })
                                       .Select(g => g.First())
                                       .ToList();

            List<string> filterList = ["insecure-deserialization",];  // example of unvanted message
            if (filterList.Count > 0)
            {
                uniqueHotspots = FilterHotspots(uniqueHotspots, filterList);
            }

            return [.. uniqueHotspots.OrderBy(x => x.Path).ThenBy(x => x.Line)];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="owner"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        private static async Task<int> GetAlertAmountAsync(HttpClient client, string owner, string project)
        {
            int alertCount = 0;
            var alertNbrResponse = await client.GetAsync($"repos/{owner}/{project}/code-scanning/alerts?page=1&per_page=1");

            if (alertNbrResponse.IsSuccessStatusCode)
            {
                string jsonString = alertNbrResponse.Content.ReadAsStringAsync().Result;
                var data = JsonSerializer.Deserialize<List<RootObject>>(jsonString, jsonSerializerOptions);

                if (data != null)
                {
                    alertCount = data[0].Number;
                }
            }

            return alertCount;
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

        private static void PrintHotspotCategories(List<GithubEntity> hotspots)
        {
            var hotspotCategories = hotspots.Select(x => x.Path.Split("/")[0] + ": " + x.Rule.Description + ": " + string.Join(", ", x.Rule.Tags)).Distinct().ToList();
            foreach (var category in hotspotCategories)
            {
                Console.WriteLine(category);
            }
        }

        private static List<GithubEntity> FilterHotspots(List<GithubEntity> unfilteredHotspots, List<string> filterList)
        {
            List<GithubEntity> filteredHotspots = [];

            foreach (var issue in filterList)
            {
                var filtered = unfilteredHotspots.Where(x => !x.Rule.Description.Contains(issue)).ToList();
                filteredHotspots.AddRange(filtered);
            }
            return [.. filteredHotspots.Distinct().OrderBy(x => x.Path).ThenBy(x => x.Line)];
        }
    }
}

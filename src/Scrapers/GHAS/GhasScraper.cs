using System.Net.Http.Headers;
using System.Text.Json;
using StaticCodeAnalysisSquared.src.Entity;
using StaticCodeAnalysisSquared.src.Scrapers.Secrets;

namespace GHAS
{
    public class GhasScraper
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
            string resultString = $"GHAS results are:\n";
            int truePositive = 0;
            int falsePositive = 0;
            int falseNegative = 0;
            int trueNegative = 0;
            int duplicate = 0;

            List<string> allCategories = allGoodBad.Select(x => x.Category).Distinct().ToList();
            List<CategoryResults> categoriesData = [];
            foreach (var category in allCategories)
            {
                categoriesData.Add(new CategoryResults(category, "tp", 0));
                categoriesData.Add(new CategoryResults(category, "fp", 0));
                categoriesData.Add(new CategoryResults(category, "tn", 0));
                categoriesData.Add(new CategoryResults(category, "fn", 0));
            }

            List<GhasEntity> allHotspots = await GetValidHotspots(project);
            
            var hotspotCategories = allHotspots.Select(x => x.Path.Split("/")[0] + ": " + x.Rule.Description + ": " + string.Join(", ", x.Rule.Tags)).Distinct().ToList();
            foreach (var category in hotspotCategories)
            {
                await Console.Out.WriteLineAsync(category);
            }

            await Console.Out.WriteLineAsync("Analysing GHAS results");

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static async Task<List<GhasEntity>> GetValidHotspots(string project)
        {
            List<GhasEntity> allHotspots = await ScrapeGhasAsync(project);
            List<GhasEntity> validHotspots = allHotspots.Where(x => x.Path.Contains("CWE")).ToList();

            await Console.Out.WriteLineAsync($"Valid security hotspots: {validHotspots.Count}");

            return validHotspots;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static async Task<List<GhasEntity>> ScrapeGhasAsync(string project)
        {
            string? owner = (project == "TestCasesCurated" ? secrets.GithubOwner1 : secrets.GithubOwner2)
                            ?? throw new ArgumentNullException("owner 2 not specified");

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

            await Console.Out.WriteLineAsync($"Loading GHAS data");
            do
            {
                var response = await client.GetAsync($"repos/{owner}/{project}/code-scanning/alerts?page={page}&per_page={itemsPerPage}");
                await Console.Out.WriteLineAsync($"Loading GHAS data page: {page}");
                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;

                    var alertResults = JsonSerializer.Deserialize<List<RootObject>>(json, jsonSerializerOptions);
                    fetchedHotspots.AddRange(collection: alertResults);
                    valid = nbrOfAlerts / itemsPerPage >= page;
                    page++;
                }
                else
                {
                    valid = false;
                }

            } while (valid);

            var convertedHotspots = GhasEntity.Convert(fetchedHotspots);
            var uniqueHotspots = convertedHotspots.GroupBy(h => new { h.Path, h.Line })
                                       .Select(g => g.First())
                                       .ToList();

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
                alertCount = data[0].Number;
            }

            return alertCount;
        }
    }
}

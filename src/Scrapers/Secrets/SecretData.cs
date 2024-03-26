using System.Reflection;
using System.Text.Json;

namespace StaticCodeAnalysisSquared.src.Scrapers.Secrets
{
    public class SecretData
    {
        public static readonly string projectRoot = Directory.GetParent(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location))?.Parent?.Parent?.FullName;
        public required string GithubToken { get; set; }
        public required string GithubUser { get; set; }
        public required string GithubOwner1 { get; set; }
        public string? GithubOwner2 { get; set; }
        public required string SonarKey { get; set; }

        public static SecretData GetAllData()
        {
            string jsonFilePath = @"src/Scrapers/Secrets/secrets.json";
            string filePath = Path.Combine(projectRoot, jsonFilePath);

            string jsonString = File.ReadAllText(filePath);
            SecretData secrets = JsonSerializer.Deserialize<SecretData>(jsonString);
            return secrets;
        }
    }
}

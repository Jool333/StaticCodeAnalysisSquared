namespace StaticCodeAnalysisSquared.src.Entity
{
    /// <summary>
    /// The base object of a security finding given from an api call to github security code scanning
    /// </summary>
    public class RootObject
    {
        public required int Number { get; set; }
        public required MostRecentInstance Most_recent_instance { get; set; }
        public required Rules Rule { get; set; }
    }
    /// <summary>
    /// The part of the base object where you find its message and location
    /// </summary>
    public class MostRecentInstance
    {
        public required Message Message { get; set; }
        public required Location Location { get; set; }
    }
    /// <summary>
    /// The message of the security finding
    /// </summary>
    public class Message
    {
        public required string Text { get; set; }
    }
    /// <summary>
    /// The location of the security finding contains the file path and the line number.
    /// </summary>
    public class Location
    {
        public required string Path { get; set; }
        public required int Start_line { get; set; }
    }
    /// <summary>
    /// The rules related to the security finding, its text description and list of tags.
    /// </summary>
    public class Rules
    {
        public required string Description { get; set; }
        public string[] Tags { get; set; } = ["No tags"];
    }
    /// <summary>
    /// A simplified entiry for a security finding where only data needed for analysis is remaining.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="line"></param>
    /// <param name="rule"></param>
    public class GithubEntity(string path, int line, Rules rule)
    {
        public string Path { get; set; } = path;
        public int Line { get; set; } = line;
        public Rules Rule { get; set; } = rule;

        /// <summary>
        /// Converts a rootobject to a githubentity
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static List<GithubEntity> Convert(List<RootObject> root)
        {
            List<GithubEntity> entities = [];
            foreach (var item in root)
            {
                entities.Add(new GithubEntity(item.Most_recent_instance.Location.Path, item.Most_recent_instance.Location.Start_line, item.Rule));
            }
            return entities;
        }
    }
}

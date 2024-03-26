namespace StaticCodeAnalysisSquared.src.Entity
{
    public class RootObject
    {
        public required int Number { get; set; }
        public required MostRecentInstance Most_recent_instance { get; set; }
    }

    public class MostRecentInstance
    {
        public required Message Message { get; set; }
        public required Location Location { get; set; }
    }

    public class Message
    {
        public required string Text { get; set; }
    }

    public class Location
    {
        public required string Path { get; set; }
        public required int Start_line { get; set; }
    }
    public class GhasEntity(string path, int line)
    {
        public string Path { get; set; } = path;
        public int Line { get; set; } = line;

        public static List<GhasEntity> Convert(List<RootObject> root)
        {
            List<GhasEntity> entities = [];
            foreach (var item in root)
            {
                entities.Add(new GhasEntity(item.Most_recent_instance.Location.Path, item.Most_recent_instance.Location.Start_line));
            }
            return entities;
        }
    }
}

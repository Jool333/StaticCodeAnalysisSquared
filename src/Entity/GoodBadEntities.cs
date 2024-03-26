namespace StaticCodeAnalysisSquared.src.Entity
{
    public class GoodBadEntity
    {
        public required string Category { get; set; }
        public required string Name { get; set; }
        public List<BadEntity>? Bad { get; set; }
        public List<GoodEntity>? Good { get; set; }
    }
    public class GoodEntity
    {
        public int Start { get; set; }
        public int End { get; set; }
    }
    public class BadEntity
    {
        public int Start { get; set; }
        public int End { get; set; }
    }
    public class CategoryResults(string category, string resultType, int counter)
    {
        public string Category { get; set; } = category;
        public string ResultType { get; set; } = resultType;
        public int Counter { get; set; } = counter;
    }
}

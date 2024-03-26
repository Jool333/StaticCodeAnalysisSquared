namespace StaticCodeAnalysisSquared.src.Entity
{
    public class Paging
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
    }

    public class Hotspot
    {
        public required string Component { get; set; }
        public int Line { get; set; }
    }

    public class Root
    {
        public required Paging Paging { get; set; }
        public List<Hotspot>? Hotspots { get; set; }
    }
}

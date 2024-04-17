namespace StaticCodeAnalysisSquared.src.Entity
{
    /// <summary>
    /// The base object of a security finding given from an api call to SonarQube.
    /// Contains its paging and potential hotspots.
    /// </summary>
    public class Root
    {
        public required Paging Paging { get; set; }
        public List<Hotspot>? Hotspots { get; set; }
    }
    /// <summary>
    /// Class containing all information regarding paging from a sonarqube api call.
    /// Contains a page count, items per page and the total amount of found object.
    /// </summary>
    public class Paging
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
    }
    /// <summary>
    /// Class containing all information regarding security hotspots from a sonarqube api call.
    /// Contains the hotspots component, its catagory, message and found line.
    /// </summary>
    public class Hotspot
    {
        public required string Component { get; set; }
        public required string SecurityCategory { get; set; }
        public required string Message { get; set; } 
        public int Line { get; set; }
    }

    
}

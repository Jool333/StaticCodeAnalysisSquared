namespace StaticCodeAnalysisSquared.src.Entity
{
    /// <summary>
    /// The Entity class for the juliet test suite data, containing what category a testcast is, 
    /// what its filename is and its good and bad lists.
    /// </summary>
    public class GoodBadEntity
    {
        public required string Category { get; set; }
        public required string Name { get; set; }
        public List<BadEntity> Bad { get; set; } = [];
        public List<GoodEntity> Good { get; set; } = [];
    }
    /// <summary>
    /// Entity for containing a start and endpoint of a segment of code.
    /// </summary>
    public class BaseEntity
    {
        public int Start { get; set; }
        public int End { get; set; }
    }
    /// <summary>
    /// An entity to contain the start and end of a good segment of code.
    /// </summary>
    public class GoodEntity : BaseEntity;
    /// <summary>
    /// An entity to contain the start and end of a bad segment of code.
    /// </summary>
    public class BadEntity : BaseEntity;
    /// <summary>
    /// A class to contain information about each test category, with each result type 
    /// and a counter for the occurance of each type.
    /// </summary>
    /// <param name="category"></param>
    /// <param name="resultType"></param>
    /// <param name="counter"></param>
    public class CategoryResults(string category, string resultType, int counter)
    {
        public string Category { get; set; } = category;
        public string ResultType { get; set; } = resultType;
        public int Counter { get; set; } = counter;
    }
}

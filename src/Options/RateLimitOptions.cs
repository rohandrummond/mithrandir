namespace mithrandir.Options;

public class RateLimitOptions
{
    public const string SectionName = "RateLimiting";
    public int FreeTierLimit { get; set; } = 10;
    public int ProTierLimit { get; set; } = 50;
    public int WindowMinutes { get; set; } = 10;
}
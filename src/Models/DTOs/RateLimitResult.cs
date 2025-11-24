namespace mithrandir.Models.DTOs;

public class RateLimitResult
{
    public bool Allowed { get; set; }
    public int Remaining { get; set; }
    public DateTimeOffset Reset { get; set; }
}
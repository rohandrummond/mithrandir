namespace mithrandir.Models.DTOs;

public class RateLimitResult
{
    public bool Allowed { get; set; }
    public int Remaining { get; set; }
    public int RetryAfterSeconds { get; set; }
}
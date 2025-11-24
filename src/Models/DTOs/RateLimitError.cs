namespace mithrandir.Models.DTOs;

public class RateLimitError
{
    public required string Error { get; set; }
    public int RetryAfterSeconds { get; init; }
}
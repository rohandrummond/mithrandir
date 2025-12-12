namespace mithrandir.Models.DTOs;

public class GetUsageResponse
{ 
    public Tier Tier { get; set; }
    public Status Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }  // 2xx responses
    public int FailedRequests { get; set; }      // 4xx + 5xx responses
    public List<EndpointUsage> EndpointUsage { get; set; } = [];
    public List<StatusCodeSummary> StatusCodeSummaries { get; set; } = [];
}

public class EndpointUsage
{
    public required string Endpoint { get; set; }
    public int Count { get; set; }
}

public class StatusCodeSummary
{
    public int StatusCode { get; set; }
    public int Count { get; set; }
}
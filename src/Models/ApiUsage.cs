namespace mithrandir.Models;

public class ApiUsage
{
  public int Id { get; set; }
  public DateTimeOffset Timestamp { get; set; }
  public required string Endpoint { get; set; }
  public required string IpAddress { get; set; }
  public int StatusCode { get; set; }

  // Foreign key for ApiKey table
  public int ApiKeyId { get; set; }  
  public ApiKey ApiKey { get; set; } = null!;
}
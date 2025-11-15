namespace mithrandir.Models;

public class ApiUsage
{
  public int Id { get; set; }
  public DateTimeOffset Timestamp { get; set; }
  public string Endpoint { get; set; }
  public string IpAddress { get; set; }
  public int StatusCode { get; set; }

  // Foreign key
  public int ApiKeyId { get; set; }  
  public ApiKey ApiKey { get; set; } = null!;
}
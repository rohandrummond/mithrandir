namespace mithrandir.Models;

public class ApiKey
{
  public int Id { get; set; }
  public required string KeyHash { get; set; }
  public required string Name { get; set; }
  public Tier Tier { get; set; }
  public Status Status { get; set; }
  public string[]? IpWhitelist { get; set; }
  public DateTimeOffset CreatedAt { get; set; }
  public DateTimeOffset? ExpiresAt { get; set; }
  public DateTimeOffset? LastUsedAt { get; set; }
}
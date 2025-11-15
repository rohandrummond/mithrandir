namespace mithrandir.Models.DTOs;

public class GenerateKeyResponse
{
  public string Key { get; set; }
  public string Name { get; set; }
  public Tier Tier { get; set; }
  public DateTimeOffset CreatedAt { get; set; }
  public DateTimeOffset? ExpiresAt { get; set; }
}
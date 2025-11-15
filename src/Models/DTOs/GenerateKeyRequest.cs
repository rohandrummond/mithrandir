namespace mithrandir.Models.DTOs;

public class GenerateKeyRequest
{
  public string Name { get; set; }
  public Tier Tier { get; set; }
  public DateTimeOffset? ExpiresAt { get; set; }
}
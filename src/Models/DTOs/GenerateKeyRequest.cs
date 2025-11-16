namespace mithrandir.Models.DTOs;

public class GenerateKeyRequest
{
  public required string Name { get; set; }
  public Tier Tier { get; set; }
  public DateTimeOffset? ExpiresAt { get; set; }
}
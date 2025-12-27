namespace mithrandir.Models.DTOs;

public class GenerateKeyResponse
{
  public required int Id { get; set; }
  public required string Key { get; set; }
  public required string Name { get; set; }
  public Tier Tier { get; set; }
  public DateTimeOffset CreatedAt { get; set; }
  public DateTimeOffset? ExpiresAt { get; set; }
}
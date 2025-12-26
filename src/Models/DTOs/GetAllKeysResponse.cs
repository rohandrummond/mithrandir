namespace mithrandir.Models.DTOs;

public class GetAllKeysResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<ApiKeyDto> Keys { get; set; } = [];
}

public class ApiKeyDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public Tier Tier { get; set; }
    public Status Status { get; set; }
    public List<string>? IpWhitelist { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }
}

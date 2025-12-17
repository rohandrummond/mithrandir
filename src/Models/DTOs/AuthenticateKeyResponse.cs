using System.Text.Json.Serialization;
namespace mithrandir.Models.DTOs;

public class AuthenticateKeyResponse
{
    public bool IsValid { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Reason { get; set; }
    public int? Id { get; set; }
    public Tier? Tier { get; set; }
    public List<string>? IpWhitelist { get; set; }
}
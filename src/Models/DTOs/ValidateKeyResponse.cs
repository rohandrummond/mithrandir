using System.Text.Json.Serialization;

namespace mithrandir.Models.DTOs;

public class ValidateKeyResponse
{
    public bool IsValid { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Reason { get; set; } 
    public Tier? Tier { get; set; }
}
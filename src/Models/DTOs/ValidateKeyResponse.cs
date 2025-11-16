namespace mithrandir.Models.DTOs;

public class ValidateKeyResponse
{
    public bool IsValid { get; set; }
    public string? Reason { get; set; } 
    public Tier? Tier { get; set; }
}
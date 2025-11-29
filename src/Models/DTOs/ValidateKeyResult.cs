namespace mithrandir.Models.DTOs;

public class ValidateKeyResult
{
    public bool IsValid { get; set; }
    public int? Id { get; set; }
    public string? Reason { get; set; } 
    public Tier? Tier { get; set; }
}
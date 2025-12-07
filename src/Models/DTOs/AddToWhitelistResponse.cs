namespace mithrandir.Models.DTOs;

public class AddToWhitelistResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string> WhitelistedIps { get; set; }
}
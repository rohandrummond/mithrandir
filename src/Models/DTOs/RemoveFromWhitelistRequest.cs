namespace mithrandir.Models.DTOs;

public class RemoveFromWhitelistRequest
{
    public required string Key { get; set; }
    public required string IpAddress { get; set; }
}
using mithrandir.Attributes;

namespace mithrandir.Models.DTOs;

public class RemoveFromWhitelistRequest
{
    public required string Key { get; set; }
    
    [ValidateIpAddress]
    public required string IpAddress { get; set; }
}
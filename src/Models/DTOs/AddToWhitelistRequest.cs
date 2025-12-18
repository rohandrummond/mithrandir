using mithrandir.Attributes;

namespace mithrandir.Models.DTOs;

public class AddToWhitelistRequest
{
    public required string Key {  get; set; }
    
    [ValidateIpAddress]
    public required string IpAddress { get; set; }
}
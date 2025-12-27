using mithrandir.Attributes;

namespace mithrandir.Models.DTOs;

public class RemoveFromWhitelistRequest
{
    public required int Id { get; set; }

    [ValidateIpAddress]
    public required string IpAddress { get; set; }
}
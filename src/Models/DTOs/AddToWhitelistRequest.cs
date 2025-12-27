using mithrandir.Attributes;

namespace mithrandir.Models.DTOs;

public class AddToWhitelistRequest
{
    public required int Id { get; set; }

    [ValidateIpAddress]
    public required string IpAddress { get; set; }
}
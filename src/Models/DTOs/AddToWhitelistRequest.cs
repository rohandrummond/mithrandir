namespace mithrandir.Models.DTOs;

public class AddToWhitelistRequest
{
    public required string Key {  get; set; }
    public required string IpAddress { get; set; }
}
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace mithrandir.Attributes;

public class ValidateIpAddressAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string ip && !IPAddress.TryParse(ip, out _))
        {
            return new ValidationResult("Invalid IP address");
        }
        return ValidationResult.Success;
    }
}
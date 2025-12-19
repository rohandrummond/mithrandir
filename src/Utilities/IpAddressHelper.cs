using System.Net;

namespace mithrandir.Utilities;

public static class IpAddressHelper
{
    // Normalizse IP addresses to a consistent format
    public static string? Normalize(string? ipString)
    {
        if (string.IsNullOrWhiteSpace(ipString))
        {
            return null;
        }

        // Parse the IP address
        if (!IPAddress.TryParse(ipString.Trim(), out var ipAddress))
        {
            return null;
        }

        // Handle IPv4 mapped IPv6 addresses
        if (ipAddress.IsIPv4MappedToIPv6)
        {
            ipAddress = ipAddress.MapToIPv4();
        }

        return ipAddress.ToString();
    }

    // Extract and normalise client IP from HTTP
    public static string? GetClientIp(HttpContext context)
    {
        // Check X-Forwarded-For header first (for proxy environments)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var firstIp = forwardedFor.Split(',')[0].Trim();
            return Normalize(firstIp);
        }

        // Fall back to direct connection IP
        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp == null)
        {
            return null;
        }

        // Handle IPv4-mapped IPv6 for direct connections
        if (remoteIp.IsIPv4MappedToIPv6)
        {
            remoteIp = remoteIp.MapToIPv4();
        }

        return remoteIp.ToString();
    }

    // Checks if a client IP matches any IP in the whitelist.
    public static bool IsInWhitelist(string? clientIp, IEnumerable<string>? whitelist)
    {
        if (string.IsNullOrEmpty(clientIp) || whitelist == null)
        {
            return false;
        }

        var normalizedClientIp = Normalize(clientIp);
        if (normalizedClientIp == null)
        {
            return false;
        }

        foreach (var whitelistedIp in whitelist)
        {
            var normalizedWhitelistedIp = Normalize(whitelistedIp);
            if (normalizedWhitelistedIp != null &&
                string.Equals(normalizedClientIp, normalizedWhitelistedIp, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}

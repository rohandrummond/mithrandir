using System.Security.Cryptography;
using System.Text;
using mithrandir.Services;
using mithrandir.Models.DTOs;
using mithrandir.Utilities;

namespace mithrandir.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService)
    {
        _logger.LogInformation("Processing request with authentication middleware");
        
        // Do not apply middleware on admin routes
        if (context.Request.Path.StartsWithSegments("/api/admin"))
        {
            _logger.LogInformation("Admin path detected, bypassing authentication: {Path}",
                context.Request.Path);
            
            await _next(context);
            return;
        }
        
        if (!context.Request.Headers.TryGetValue("X-Api-Key", out var apiKeyValue))
        {
            _logger.LogWarning("Authentication failed due to missing X-Api-Key header");
            
            // Handle missing API key
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync("You shall not pass (missing API key)");
            return;
        }

        var apiKey = apiKeyValue.ToString();

        // Validate key
        var result = await apiKeyService.AuthenticateKeyAsync(new AuthenticateKeyRequest() { Key = apiKey });

        if (!result.IsValid)
        {
            _logger.LogWarning("Authentication failed due to invalid API key");
            
            // Handle invalid API key
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("You shall not pass (invalid API key)");
            return;
        }
        
        // Check IP whitelist
        var clientIp = IpAddressHelper.GetClientIp(context);

        if (string.IsNullOrEmpty(clientIp))
        {
            _logger.LogWarning("Authentication failed due inability to determine client IP. Key ID = {KeyId}",
                result.Id);

            // Handle undetermined client IP
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("Unable to determine client IP address");
            return;
        }

        if (!IpAddressHelper.IsInWhitelist(clientIp, result.IpWhitelist))
        {
            _logger.LogWarning("Authentication failed due to IP not being whitelisted. Key ID = {KeyId},  Client IP = {ClientIp}",
                result.Id , clientIp);

            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("IP address has not been whitelisted");
            return;
        }
        
        _logger.LogInformation(
            "Request authenticated. Key ID = {KeyId}, Tier: {Tier}, IP: {ClientIp}",
            result.Id , result.Tier, clientIp);

        // Handle valid API key 
        var redisHash = GetSha256Hash(apiKey);
        context.Items["KeyHash"] = redisHash;
        context.Items["Tier"] = result.Tier;
        context.Items["Id"] = result.Id;
        await _next(context);

    }
    
    // Hash API keys so they can be stored in context for Redis
    private static string GetSha256Hash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashedBytes = SHA256.HashData(bytes);
        return Convert.ToBase64String(hashedBytes);
    }
}
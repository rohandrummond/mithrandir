using System.Security.Cryptography;
using System.Text;
using mithrandir.Services;
using mithrandir.Models.DTOs;

namespace mithrandir.Middleware;

public class AuthenticationMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService)
    {
        
        // Do not apply middleware on admin routes
        if (context.Request.Path.StartsWithSegments("/api/admin"))
        {
            await _next(context);
            return;
        }
        
        if (!context.Request.Headers.TryGetValue("X-Api-Key", out var apiKeyValue))
        {
            // Handle missing API key
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync("You shall not pass (missing API key)");
            return;
        }

        var apiKey = apiKeyValue.ToString();

        // Validate key
        var result = await apiKeyService.ValidateKeyAsync(new ValidateKeyRequest { Key = apiKey });

        if (!result.IsValid)
        {
            // Handle invalid API key
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("You shall not pass (invalid API key)");
            return;
        }
        
        // Check IP whitelist
        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        
        if (string.IsNullOrEmpty(clientIp))
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("Unable to determine client IP address");
            return;
        }
        
        if (!result.IpWhitelist.Contains(clientIp))
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("IP address has not been whitelisted");
            return;
        }

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
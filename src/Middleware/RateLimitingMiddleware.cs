using Microsoft.EntityFrameworkCore;

using mithrandir.Data;
using mithrandir.Services;
using mithrandir.Models;
using mithrandir.Models.DTOs;

namespace mithrandir.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IRateLimitService rateLimitService, MithrandirDbContext dbContext)
    {
        
        // Do not apply middleware on admin routes
        if (context.Request.Path.StartsWithSegments("/api/admin"))
        {
            await _next(context);
            return;
        }
        
        // Get hash and tier from HTTP context
        var keyHash = context.Items["KeyHash"] as string;
        var tier = context.Items["Tier"] as Tier?;
        
        // Check for missing hash or tier
        if (keyHash == null || tier == null)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("Server error, API key context not found");
            return;
        }
        
        var result = await rateLimitService.CheckAndIncrementAsync(keyHash, tier.Value);

        if (!result.Allowed)
        {
            // API key has hit rate limit and request should be bounced
            var error = new RateLimitError
            {
                Error = "Rate limit exceeded",
                RetryAfterSeconds = result.RetryAfterSeconds
            };
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            context.Response.Headers.RetryAfter = error.RetryAfterSeconds.ToString();
            await context.Response.WriteAsJsonAsync(error);
            return;
        }
        
        // API key is within limit and we can continue
        await _next(context);
        
        // Store and validate API key and IP address
        var apiKeyId = context.Items["Id"] as int?;
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        if (apiKeyId == null)
        {
            await Console.Error.WriteLineAsync("Unable to log key usage due to missing key id");
            return; 
        }

        if (string.IsNullOrEmpty(ipAddress))
        {
            await Console.Error.WriteLineAsync("Warning: Unable to log IP address for request");
            ipAddress = "Unknown";
        }
        
        // Create ApiUsage object
        var apiUsage = new ApiUsage
        {
            Timestamp = DateTimeOffset.UtcNow,
            Endpoint =  context.Request.Path,
            IpAddress = ipAddress,
            StatusCode =  context.Response.StatusCode,
            ApiKeyId = apiKeyId.Value,
        };
        
        // Record usage in database
        try
        {
            await dbContext.ApiUsages.AddAsync(apiUsage);
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) 
        {
            // Handle database errors
            await Console.Error.WriteLineAsync($"Failed to record key usage in database: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Handle other errors
            await Console.Error.WriteLineAsync($"Unexpected error occured while recording key usage: {ex.Message}");
        }
        
    }

}
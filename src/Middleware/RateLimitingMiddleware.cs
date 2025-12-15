using Microsoft.EntityFrameworkCore;

using mithrandir.Data;
using mithrandir.Services;
using mithrandir.Models;
using mithrandir.Models.DTOs;

namespace mithrandir.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(RequestDelegate next,  ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IRateLimitService rateLimitService, MithrandirDbContext dbContext)
    {
        _logger.LogInformation("Processing request with rate limiting middleware");
        
        // Do not apply middleware on admin routes
        if (context.Request.Path.StartsWithSegments("/api/admin"))
        {
            _logger.LogInformation("Admin path detected, bypassing rate limiting: {Path}",
                context.Request.Path);
            
            await _next(context);
            return;
        }
        
        // Get hash and tier from HTTP context
        var keyHash = context.Items["KeyHash"] as string;
        var tier = context.Items["Tier"] as Tier?;
        
        // Check for missing hash or tier
        if (keyHash == null || tier == null)
        {
            _logger.LogError("Rate limiting middleware failed as key hash or tier is missing from HTTP context");
            
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("Server error, API key context not found");
            return;
        }
        
        var result = await rateLimitService.CheckAndIncrementAsync(keyHash, tier.Value);

        if (!result.Allowed)
        {
            _logger.LogWarning("Rate limit has been hit, bouncing request. Hash =  {KeyHash}, Tier = {Tier}",
                keyHash, tier);
            
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
            _logger.LogError("Unable to log usage as key ID is missing from HTTP context");
            return; 
        }

        if (string.IsNullOrEmpty(ipAddress))
        {
            _logger.LogError("Unable to log IP address in usage records due to inability to determine client IP");
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
        
        _logger.LogDebug(
            "Recording API usage. Key ID = {KeyId}, IP Address = {IpAddress}, Endpoint: {Endpoint}",
            apiUsage.ApiKeyId, apiUsage.IpAddress, apiUsage.Endpoint);
        
        // Record usage in database
        try
        {
            await dbContext.ApiUsages.AddAsync(apiUsage);
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) 
        {
            _logger.LogError(ex, "Database error while recording key usage: Key ID = {KeyId}, IP Address = {IpAddress}, Endpoint: {Endpoint}",
                apiUsage.ApiKeyId, apiUsage.IpAddress, apiUsage.Endpoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while recording key usage: Key ID = {KeyId}, IP Address = {IpAddress}, Endpoint: {Endpoint}",
                apiUsage.ApiKeyId, apiUsage.IpAddress, apiUsage.Endpoint);
        }
        
    }

}
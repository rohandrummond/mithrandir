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

        // Check if this is an admin route
        var isAdminRoute = context.Request.Path.StartsWithSegments("/api/admin");

        if (isAdminRoute)
        {
            // Rate limit admin routes by IP
            var adminClientIp = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim()
                ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var adminResult = await rateLimitService.CheckAndIncrementAsync($"admin:{adminClientIp}", Tier.Pro);

            if (!adminResult.Allowed)
            {
                _logger.LogWarning("Admin rate limit exceeded for IP: {ClientIp}", adminClientIp);

                var error = new RateLimitError
                {
                    Error = "Rate limit exceeded",
                    RetryAfterSeconds = adminResult.RetryAfterSeconds
                };
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json";
                context.Response.Headers.RetryAfter = error.RetryAfterSeconds.ToString();
                await context.Response.WriteAsJsonAsync(error);
                return;
            }

            await _next(context);
            return;
        }

        // Get hash and tier from HTTP context for non admin routes
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
        
        // Capture usage before next middleware
        var apiKeyId = context.Items["Id"] as int?;
        var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim()
            ?? context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var endpoint = context.Request.Path.ToString();
        var timestamp = DateTimeOffset.UtcNow;

        // API key is within limit and we can continue
        await _next(context);

        // Log usage after response (best-effort, non-blocking)
        if (apiKeyId == null)
        {
            _logger.LogError("Unable to log usage as key ID is missing from HTTP context");
            return;
        }

        // Create ApiUsage object
        var apiUsage = new ApiUsage
        {
            Timestamp = timestamp,
            Endpoint = endpoint,
            IpAddress = ipAddress,
            StatusCode = context.Response.StatusCode,
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
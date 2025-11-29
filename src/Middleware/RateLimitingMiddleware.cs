using mithrandir.Services;
using mithrandir.Models;
using mithrandir.Models.DTOs;

namespace mithrandir.Middleware;

public class RateLimitingMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, IRateLimitService rateLimitService)
    {
        
        // Implement only on restricted route for now
        if (!context.Request.Path.StartsWithSegments("/api/keys/restricted"))
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
    }


}
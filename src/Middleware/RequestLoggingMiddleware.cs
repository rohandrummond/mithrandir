using System.Diagnostics;
using mithrandir.Services;

namespace mithrandir.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Start timing request
        var stopwatch = Stopwatch.StartNew();
        
        // Log start
        _logger.LogInformation("Request started: {Method} {Path}",  context.Request.Method, context.Request.Path);
        
        // Wait until all middleware and controller executes
        await _next(context);
        
        // Stop timer
        stopwatch.Stop();
        
        // Record status code and elapsed milliseconds
        var statusCode = context.Response.StatusCode;
        var elapsedMs = stopwatch.ElapsedMilliseconds;

        // Set log level and info based on status code
        if (statusCode >= 500)
        {
           _logger.LogError("Request failed: {Method} {Path} with {StatusCode} in {ElapsedMs}ms", 
               context.Request.Method, context.Request.Path, statusCode, elapsedMs); 
        } 
        else if (statusCode >= 400)
        {
            _logger.LogWarning("Request rejected: {Method} {Path} with {StatusCode} in {ElapsedMs}ms", 
                context.Request.Method, context.Request.Path, statusCode, elapsedMs); 
        }
        else
        {
            _logger.LogInformation("Request completed: {Method} {Path} with {StatusCode} in {ElapsedMs}ms", 
                context.Request.Method, context.Request.Path, statusCode, elapsedMs);
        }
    }
}
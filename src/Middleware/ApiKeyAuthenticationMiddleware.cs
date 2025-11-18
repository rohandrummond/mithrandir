using Microsoft.EntityFrameworkCore.Metadata.Internal;
using mithrandir.Services;
using mithrandir.Models.DTOs;

namespace mithrandir.Middleware;

public class ApiKeyAuthenticationMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService)
    {
        if (!context.Request.Headers.TryGetValue("X-Api-Key", out var apiKeyValue))
        {
            // Handle missing API key
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync("{\"error\": \"API key is required\"}");
            return;
        };

        var apiKey = apiKeyValue.ToString();

        // Validate key
        var result = await apiKeyService.ValidateKeyAsync(new ValidateKeyRequest { Key = apiKey });

        if (!result.IsValid)
        {
            // Handle invalid API key
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\": \"API key is invalid\"}");
            return;
        }

        // Handle valid API key 
        await _next(context);

    }
}
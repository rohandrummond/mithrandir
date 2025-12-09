namespace mithrandir.Middleware;

public class AdminMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        
        var path = context.Request.Path;

        if (path != null && path.StartsWithSegments("/admin"))
        {

            if (!context.Request.Headers.TryGetValue("X-Admin-Key", out var providedKey))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync("Admin key required");
            }
            
        }
        
        await _next(context);
        
    }
}
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace mithrandir.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireAdminKeyAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Get admin key 
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var adminKey = configuration["AdminApiKey"];
        
        // Check if admin key is available
        if (string.IsNullOrEmpty(adminKey))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "Admin key not configured on server"
            });
            return;
        }

        // Check if the request has the X-Admin-Key header
        if (!context.HttpContext.Request.Headers.TryGetValue("X-Admin-Key", out var providedKey))
        {
            context.Result = new UnauthorizedObjectResult(new 
            { 
                error = "Admin key required" 
            });
            return;
        }

        // Compare provided key with configured admin key
        // Constant time comparisom used to prevent timing attacks
        var providedBytes = Encoding.UTF8.GetBytes(providedKey!);
        var adminBytes = Encoding.UTF8.GetBytes(adminKey);
        if (!CryptographicOperations.FixedTimeEquals(providedBytes, adminBytes))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "Invalid admin key"
            });
            return;
        }

        // Mark request as authorized
        context.HttpContext.Items["IsAdmin"] = true;
        
    }
}
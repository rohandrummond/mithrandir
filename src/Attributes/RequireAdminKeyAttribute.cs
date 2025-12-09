using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace mithrandir.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireAdminKeyAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        
        // Get the admin key from environment variable
        // Check if admin key is available and return error otherwise

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
        if (providedKey != adminKey)
        {
            context.Result = new UnauthorizedObjectResult(new 
            { 
                error = "Invalid admin key" 
            });
            return;
        }

        // Success! Mark the request as admin for other middleware
        context.HttpContext.Items["IsAdmin"] = true;
        
    }
}
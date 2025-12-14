using Microsoft.AspNetCore.Mvc;
using mithrandir.Services;
using mithrandir.Attributes;
using mithrandir.Models.DTOs;

namespace mithrandir.Controllers
{
    [ApiController]
    [Route("api/keys")]
    public class ApiKeysController(IApiKeyService keyService) : ControllerBase
    {

        private readonly IApiKeyService _keyService = keyService;
        
        // Validate an API key 
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateKey([FromBody] ValidateKeyRequest request)
        {
            // Check that key is not null
            if (string.IsNullOrEmpty(request.Key))
            {
                return BadRequest("Key is required");
            }
            
            // Check that body and header match
            var key = HttpContext.Items["ApiKey"]?.ToString();
            if (request.Key != key)
            {
                return Unauthorized("You can't validate someone else's key"); 
            }

            try
            {
                // Check if key is valid and send response
                var result = await _keyService.ValidateKeyAsync(request);
                var response = new ValidateKeyResponse
                {
                    IsValid = result.IsValid,
                    Reason = result.Reason,
                    Tier = result.Tier
                };
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                // Return error
                return StatusCode(500, new { error = ex.Message });
            }
            
        }

        // Revoke an API key
        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeKey([FromBody] RevokeKeyRequest request)
        {
            // Check that key is not null
            if (string.IsNullOrEmpty(request.Key))
            {
                return BadRequest("Key is required");
            }
        
            try
            {
                // Delete key and send response
                var result = await _keyService.RevokeKeyAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                // Return error
                return StatusCode(500, new { error = ex.Message });
            }
        }
        
        // Get usage for API key
        [HttpPost("usage")]
        public async Task<IActionResult> GetUsage([FromBody] GetUsageRequest request)
        {
            if (string.IsNullOrEmpty(request.Key))
            {
                return BadRequest("Key is required");
            }

            try
            {
                var result = await _keyService.GetUsageAsync(request);
                
                if (result == null)
                {
                    return NotFound(new { error = "API key not found" });
                }
                
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        
    }
    
    // Admin only controller
    [ApiController]
    [Route("api/admin/keys")]
    [RequireAdminKey]
    public class AdminKeysController(IApiKeyService keyService)  : ControllerBase
    {
        
        private readonly IApiKeyService _keyService = keyService;
        
        // Generate an API key
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateKey([FromBody] GenerateKeyRequest request)
        {
            
            // Check that Name is not empty
            if (string.IsNullOrEmpty(request.Name))
            {
                return BadRequest("Name is required");
            }
            
            // Check Name length
            if (request.Name.Length > 100)
            {
                return BadRequest("Name must not exceed 100 characters");
            }
            
            // Check expireAt time is in the future
            if (request.ExpiresAt != null && request.ExpiresAt < DateTimeOffset.UtcNow)
            {
                return BadRequest("ExpiresAt value must be in the future");
            }
            
            try
            {
                // Generate key, save hash and return response
                var result = await _keyService.GenerateKeyAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                // Return error
                return StatusCode(500, new { error = ex.Message });
            }
        }
        
        // Delete an API key
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteKey([FromBody] DeleteKeyRequest request)
        {
            // Check that key is not null
            if (string.IsNullOrEmpty(request.Key))
            {
                return BadRequest("Key is required");
            }

            try
            {
                // Delete key and send response
                var result = await _keyService.DeleteKeyAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        
        // Add IP address to whitelist
        [HttpPost("whitelist/add")]
        public async Task<IActionResult> AddToWhitelist([FromBody] AddToWhitelistRequest request)
        {
            if (string.IsNullOrEmpty(request.Key))
            {
                return BadRequest("Key is required");
            }

            try
            {
                var result = await _keyService.AddToWhitelistAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Remove IP address from whitelist
        [HttpDelete("whitelist/remove")]
        public async Task<IActionResult> RemoveFromWhitelist([FromBody] RemoveFromWhitelistRequest request)
        {
            if (string.IsNullOrEmpty(request.Key))
            {
                return BadRequest("Key is required");
            }

            try
            {
                var result = await _keyService.RemoveFromWhitelistAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
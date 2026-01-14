using Microsoft.AspNetCore.Mvc;
using mithrandir.Services;
using mithrandir.Attributes;
using mithrandir.Models.DTOs;

namespace mithrandir.Controllers
{
    // Admin only controller
    [ApiController]
    [Route("api/admin/keys")]
    [RequireAdminKey]
    public class AdminKeysController(IApiKeyService keyService)  : ControllerBase
    {

        private readonly IApiKeyService _keyService = keyService;

        // Get all API keys
        [HttpGet]
        public async Task<IActionResult> GetAllKeys()
        {
            try
            {
                var result = await _keyService.GetAllKeysAsync();
                return Ok(result);
            }
            catch (InvalidOperationException)
            {
                return StatusCode(500, new GetAllKeysResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving API keys"
                });
            }
        }

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
            catch (InvalidOperationException)
            {
                // Return error
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }

        // Delete an API key
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteKey([FromBody] DeleteKeyRequest request)
        {
            // Check that ID is valid
            if (request.Id <= 0)
            {
                return BadRequest("Valid Id is required");
            }

            try
            {
                // Delete key and send response
                var result = await _keyService.DeleteKeyAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException)
            {
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }

        // Add IP address to whitelist
        [HttpPost("whitelist/add")]
        public async Task<IActionResult> AddToWhitelist([FromBody] AddToWhitelistRequest request)
        {
            if (request.Id <= 0)
            {
                return BadRequest("Valid key ID is required");
            }

            try
            {
                var result = await _keyService.AddToWhitelistAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException)
            {
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }

        // Remove IP address from whitelist
        [HttpDelete("whitelist/remove")]
        public async Task<IActionResult> RemoveFromWhitelist([FromBody] RemoveFromWhitelistRequest request)
        {
            if (request.Id <= 0)
            {
                return BadRequest("Valid key ID is required");
            }

            try
            {
                var result = await _keyService.RemoveFromWhitelistAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException)
            {
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }
    }
}

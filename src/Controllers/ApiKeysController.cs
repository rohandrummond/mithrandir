using Microsoft.AspNetCore.Mvc;
using mithrandir.Services;
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
            catch (InvalidOperationException)
            {
                // Return error
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }

        }

        // Revoke an API key
        [HttpPatch("revoke")]
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
            catch (InvalidOperationException)
            {
                // Return error
                return StatusCode(500, new { error = "An unexpected error occurred" });
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
            catch (InvalidOperationException)
            {
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }

    }
}

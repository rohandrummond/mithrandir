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

        // api/keys/generate [POST]
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
    }
}
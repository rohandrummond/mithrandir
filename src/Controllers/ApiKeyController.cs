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

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateKey([FromBody] GenerateKeyRequest request)
        {
            var result = await _keyService.GenerateKeyAsync(request);
            return Ok(result);
        }
    }
}
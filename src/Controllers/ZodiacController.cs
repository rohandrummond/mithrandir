using Microsoft.AspNetCore.Mvc;
using mithrandir.Services;

namespace mithrandir.Controllers
{
    [ApiController]
    [Route("api/zodiac")]
    public class ZodiacController(IZodiacService zodiacService) : ControllerBase
    {
        private readonly IZodiacService _zodiacService = zodiacService;

        [HttpGet]
        public IActionResult GetZodiac([FromQuery] string? dob)
        {
            if (string.IsNullOrEmpty(dob))
            {
                return BadRequest("Date of birth is required. Use ?dob=YYYY-MM-DD");
            }

            if (!DateOnly.TryParse(dob, out var dateOfBirth))
            {
                return BadRequest("Invalid date format. Use YYYY-MM-DD");
            }

            var response = _zodiacService.GenerateResponse(dateOfBirth);
            return Content(response, "text/plain");
        }
    }
}

using BackOfficeBlazor.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace POSAPI.Controllers
{
    [ApiController]
    [Route("api/print-agent")]
    public class PrintAgentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IPrinterConfigService _printers;

        public PrintAgentController(IConfiguration configuration, IPrinterConfigService printers)
        {
            _configuration = configuration;
            _printers = printers;
        }

        [HttpGet("printers/{id:int}")]
        public async Task<IActionResult> GetPrinter(int id, [FromQuery] string locationCode, [FromHeader(Name = "X-Agent-Key")] string? agentKey)
        {
            if (!IsAgentKeyValid(agentKey))
                return Unauthorized();

            var result = await _printers.GetAsync(id);
            if (!result.Success || result.Data == null)
                return NotFound(result);

            if (!string.Equals(result.Data.LocationCode, locationCode, StringComparison.OrdinalIgnoreCase))
                return Forbid();

            return Ok(result);
        }

        [HttpGet("health")]
        public IActionResult Health([FromHeader(Name = "X-Agent-Key")] string? agentKey)
        {
            if (!IsAgentKeyValid(agentKey))
                return Unauthorized();

            return Ok(new { ok = true });
        }

        private bool IsAgentKeyValid(string? key)
        {
            var expected = _configuration["PrintAgent:SharedKey"];
            return !string.IsNullOrWhiteSpace(expected) && string.Equals(key, expected, StringComparison.Ordinal);
        }
    }
}

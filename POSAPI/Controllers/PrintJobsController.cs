using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSAPI.Services;

namespace POSAPI.Controllers
{
    [ApiController]
    [Route("api/print-jobs")]
    public class PrintJobsController : ControllerBase
    {
        private readonly IPrintJobService _service;
        private readonly IPrintJobDispatcher _dispatcher;
        private readonly IConfiguration _configuration;

        public PrintJobsController(
            IPrintJobService service,
            IPrintJobDispatcher dispatcher,
            IConfiguration configuration)
        {
            _service = service;
            _dispatcher = dispatcher;
            _configuration = configuration;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Enqueue([FromBody] PrintJobRequestDto request)
        {
            var result = await _service.EnqueueAsync(request);
            if (result.Success && result.Data != null)
                await _dispatcher.DispatchAsync(result.Data);

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] PrintJobStatusUpdateDto update, [FromHeader(Name = "X-Agent-Key")] string? agentKey)
        {
            var expected = _configuration["PrintAgent:SharedKey"];
            if (string.IsNullOrWhiteSpace(expected) || !string.Equals(expected, agentKey, StringComparison.Ordinal))
                return Unauthorized();

            return Ok(await _service.UpdateStatusAsync(update));
        }
    }
}

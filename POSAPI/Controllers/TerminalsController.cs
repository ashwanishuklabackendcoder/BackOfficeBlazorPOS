using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POSAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/terminals")]
    public class TerminalsController : ControllerBase
    {
        private readonly ITerminalPrinterService _service;

        public TerminalsController(ITerminalPrinterService service)
        {
            _service = service;
        }

        [HttpGet("{terminalCode}/printers")]
        public async Task<IActionResult> GetPrinters(string terminalCode, [FromQuery] string locationCode)
            => Ok(await _service.GetPrintersForTerminalAsync(terminalCode, locationCode));

        [HttpPut("{terminalCode}/printers")]
        public async Task<IActionResult> SavePrinters(string terminalCode, [FromBody] TerminalPrinterAssignmentDto dto)
        {
            dto.TerminalCode = terminalCode;
            return Ok(await _service.SaveAssignmentAsync(dto));
        }
    }
}

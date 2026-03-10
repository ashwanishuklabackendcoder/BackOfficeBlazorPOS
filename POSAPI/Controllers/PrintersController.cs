using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSAPI.Services;

namespace POSAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/printers")]
    public class PrintersController : ControllerBase
    {
        private readonly IPrinterConfigService _service;
        private readonly IPrintJobDispatcher _dispatcher;

        public PrintersController(IPrinterConfigService service, IPrintJobDispatcher dispatcher)
        {
            _service = service;
            _dispatcher = dispatcher;
        }

        [HttpGet]
        public async Task<IActionResult> GetByLocation([FromQuery] string locationCode)
            => Ok(await _service.GetByLocationAsync(locationCode));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
            => Ok(await _service.GetAsync(id));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PrinterConfigDto dto)
            => Ok(await _service.SaveAsync(dto));

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] PrinterConfigDto dto)
        {
            dto.Id = id;
            return Ok(await _service.SaveAsync(dto));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
            => Ok(await _service.DeleteAsync(id));

        [HttpPost("test/{id:int}")]
        public async Task<IActionResult> TestPrint(int id, [FromBody] PrinterTestRequestDto request)
        {
            var result = await _service.EnqueueTestPrintAsync(id, request.TerminalCode);
            if (result.Success && result.Data != null)
                await _dispatcher.DispatchAsync(result.Data);

            return Ok(result);
        }
    }
}

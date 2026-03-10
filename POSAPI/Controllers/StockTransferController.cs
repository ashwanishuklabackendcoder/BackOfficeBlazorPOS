using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace POSAPI.Controllers
{
    [ApiController]
    [Route("api/stock-transfer")]
    public class StockTransferController : ControllerBase
    {
        private readonly IStockTransferService _service;

        public StockTransferController(IStockTransferService service)
        {
            _service = service;
        }

        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] StockTransferInputDto dto)
        {
            await _service.ApplyTransferAsync(dto);
            return Ok();
        }
    }

}

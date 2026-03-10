using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace POSAPI.Controllers
{
    [ApiController]
    [Route("api/stock")]
    public class StockController : ControllerBase
    {
        private readonly IStockInputService _service;

        public StockController(IStockInputService service)
        {
            _service = service;
        }

        [HttpPost("input")]
        public async Task<IActionResult> InputStock([FromBody] StockInputDto dto)
        {
            var result = await _service.SaveAsync(dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("current/{partNumber}")]
        public async Task<IActionResult> GetCurrentStock(string partNumber)
        {
            var stock = await _service.GetCurrentStockAsync(partNumber);
            return Ok(stock);
        }

        [HttpGet("history/{partNumber}")]
        public async Task<IActionResult> GetHistory(string partNumber)
        {
            var history = await _service.GetStockHistoryAsync(partNumber);
            return Ok(history);
        }

        [HttpGet("levels/{partNumber}")]
        public async Task<IActionResult> GetStockLevels(string partNumber)
        {
            var data = await _service.GetStockLevelsAsync(partNumber);
            return Ok(data);
        }
        [HttpGet("available-serials/{partNumber}/{location}")]
        public async Task<IActionResult> GetAvailableStockNumbers(string partNumber,string location)
        {
            var list = await _service.GetAvailableStockNumbersAsync(
                partNumber, location);

            return Ok(list);
        }

        //[HttpGet("available-serials/{partNumber}/{location}")]
        //public async Task<IActionResult> GetAvailableSerials(string partNumber, string location)
        //{
        //    var list = await _service.GetAvailableSerialsAsync(
        //        partNumber, location);

        //    return Ok(list);
        //}


    }
}

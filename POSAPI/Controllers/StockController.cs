using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSAPI.Security;

namespace POSAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/stock")]
    public class StockController : ControllerBase
    {
        private readonly IStockInputService _service;
        private readonly ApiAccessService _access;
        private readonly ILogger<StockController> _logger;

        public StockController(
            IStockInputService service,
            ApiAccessService access,
            ILogger<StockController> logger)
        {
            _service = service;
            _access = access;
            _logger = logger;
        }

        [HttpPost("input")]
        public async Task<IActionResult> InputStock([FromBody] StockInputDto dto)
        {
            if (!await _access.HasAnyPermissionAsync(User, PermissionKeys.StockInput))
                return Forbid();

            if (dto == null)
            {
                return BadRequest(new StockInputResultDto
                {
                    Success = false,
                    Message = "Stock input details are required."
                });
            }

            if (string.IsNullOrWhiteSpace(dto.PartNumber))
            {
                return BadRequest(new StockInputResultDto
                {
                    Success = false,
                    Message = "Part number is required."
                });
            }

            if (dto.Quantity <= 0)
            {
                return BadRequest(new StockInputResultDto
                {
                    Success = false,
                    Message = "Quantity must be greater than zero."
                });
            }

            try
            {
                var result = await _service.SaveAsync(dto);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stock input failed for part {PartNumber}", dto.PartNumber);
                return BadRequest(new StockInputResultDto
                {
                    Success = false,
                    Message = "Stock input could not be saved. Please check the entry and try again."
                });
            }
        }

        [HttpGet("current/{partNumber}")]
        public async Task<IActionResult> GetCurrentStock(string partNumber)
        {
            if (!await CanReadStockAsync())
                return Forbid();

            if (string.IsNullOrWhiteSpace(partNumber))
                return BadRequest(0);

            var stock = await _service.GetCurrentStockAsync(partNumber);
            return Ok(stock);
        }

        [HttpGet("history/{partNumber}")]
        public async Task<IActionResult> GetHistory(string partNumber)
        {
            if (!await CanReadStockAsync())
                return Forbid();

            if (string.IsNullOrWhiteSpace(partNumber))
                return BadRequest(new List<StockHistoryDto>());

            var history = await _service.GetStockHistoryAsync(partNumber);
            return Ok(history);
        }

        [HttpGet("levels/{partNumber}")]
        public async Task<IActionResult> GetStockLevels(string partNumber)
        {
            if (!await CanReadStockAsync())
                return Forbid();

            if (string.IsNullOrWhiteSpace(partNumber))
                return BadRequest(new List<ProductStockLevelDto>());

            var data = await _service.GetStockLevelsAsync(partNumber);
            return Ok(data);
        }

        [HttpGet("available-serials/{partNumber}/{location}")]
        public async Task<IActionResult> GetAvailableStockNumbers(string partNumber, string location)
        {
            if (!await CanReadStockAsync())
                return Forbid();

            if (string.IsNullOrWhiteSpace(partNumber) || string.IsNullOrWhiteSpace(location))
                return BadRequest(new List<StockNumberDto>());

            var list = await _service.GetAvailableStockNumbersAsync(partNumber, location);
            return Ok(list);
        }

        private Task<bool> CanReadStockAsync()
        {
            return _access.HasAnyPermissionAsync(
                User,
                PermissionKeys.StockInput,
                PermissionKeys.Till,
                PermissionKeys.Layaway,
                PermissionKeys.ProductAdd,
                PermissionKeys.ProductList);
        }
    }
}

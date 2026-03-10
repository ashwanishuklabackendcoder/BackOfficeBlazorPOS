using BackOfficeBlazor.Admin.Services.Implementations;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace POSAPI.Controllers
{
    [ApiController]
    [Route("api/product")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;
        public ProductController(IProductService service) => _service = service;

        [HttpGet("GetProduct")]
        public async Task<IActionResult> Get(string partNumber)
            => Ok(await _service.GetAsync(partNumber));

        [HttpGet("GetMFRPartNo")]
        public async Task<IActionResult> GetMFRPartNo(string MfrpartNumber)
         => Ok(await _service.GetMFRPartNo(MfrpartNumber));
        [HttpGet("GetBarcode")]
        public async Task<IActionResult> GetBarcode(string Barcode)
         => Ok(await _service.GetBarcode(Barcode));

        [HttpGet("GetProductGroup")]
        public async Task<IActionResult> GetProductGroup(string partNumber)
            => Ok(await _service.GetGroupAsync(partNumber));

        //[HttpGet("GetProducts")]
        //public async Task<IActionResult> GetProducts([FromQuery] ProductFilterDto filter)
        // => Ok(await _service.GetAllAsync(filter));

        [HttpGet("GetProducts")]
        public async Task<IActionResult> GetProducts([FromQuery] ProductFilterDto filter)
          => Ok(await _service.GetAllAsync(filter));

        [HttpPost("SaveProduct")]
        public async Task<IActionResult> Save([FromBody] ProductDto dto)
            => Ok(await _service.SaveAsync(dto));

        [HttpPost("SaveGroupProduct")]
        public async Task<IActionResult> SaveGroupProduct([FromBody] GroupProductDto dto)
        {
            var result = await _service.SaveGroupProduct(dto);
            return Ok(result);
        }

    }
}

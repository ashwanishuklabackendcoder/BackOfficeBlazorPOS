using BackOfficeBlazor.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace POSAPI.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IComboService _comboService;

        public ProductsController(IComboService comboService)
        {
            _comboService = comboService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? term, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
            => Ok(await _comboService.SearchProducts(term, page, pageSize));
    }
}

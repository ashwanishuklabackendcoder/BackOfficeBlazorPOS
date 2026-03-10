using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace POSAPI.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService _service;

        public ReportsController(IReportsService service)
        {
            _service = service;
        }

        [HttpPost("customer-sales-returns")]
        public async Task<IActionResult> GetCustomerSalesReturns([FromBody] CustomerSalesReturnReportRequestDto request)
        {
            if (request.From > request.To)
                return BadRequest(ApiResponse<List<CustomerSalesReturnLineDto>>.Fail("Invalid date range"));

            var rows = await _service.GetCustomerSalesReturnsAsync(request);

            return Ok(ApiResponse<List<CustomerSalesReturnLineDto>>.Ok(rows));
        }
    }
}

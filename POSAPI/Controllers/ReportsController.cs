using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSAPI.Security;

namespace POSAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService _service;
        private readonly ApiAccessService _access;

        public ReportsController(IReportsService service, ApiAccessService access)
        {
            _service = service;
            _access = access;
        }

        [HttpPost("customer-sales-returns")]
        public async Task<IActionResult> GetCustomerSalesReturns([FromBody] CustomerSalesReturnReportRequestDto request)
        {
            if (!await _access.HasAnyPermissionAsync(User, PermissionKeys.CustomerSalesReturn))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<List<CustomerSalesReturnLineDto>>.Fail("You do not have permission to view sales and returns reports."));
            }

            if (request == null)
            {
                return BadRequest(ApiResponse<List<CustomerSalesReturnLineDto>>.Fail(
                    "Report criteria are required."));
            }

            if (request.From > request.To)
            {
                return BadRequest(ApiResponse<List<CustomerSalesReturnLineDto>>.Fail(
                    "Invalid date range."));
            }

            var rows = await _service.GetCustomerSalesReturnsAsync(request);
            return Ok(ApiResponse<List<CustomerSalesReturnLineDto>>.Ok(rows, "Report loaded."));
        }
    }
}

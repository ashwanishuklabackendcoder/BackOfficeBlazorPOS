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

        [HttpPost("stock-position")]
        public async Task<IActionResult> GetStockPosition([FromBody] StockPositionReportRequestDto request)
        {
            if (!await _access.HasAnyPermissionAsync(User, PermissionKeys.StockInput, PermissionKeys.CustomerSalesReturn))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<List<StockPositionLineDto>>.Fail("You do not have permission to view stock position reports."));
            }

            if (request == null)
            {
                return BadRequest(ApiResponse<List<StockPositionLineDto>>.Fail(
                    "Report criteria are required."));
            }

            var rows = await _service.GetStockPositionAsync(request);
            return Ok(ApiResponse<List<StockPositionLineDto>>.Ok(rows, "Report loaded."));
        }

        [HttpPost("major-item-sales")]
        public async Task<IActionResult> GetMajorItemSales([FromBody] MajorItemSalesReportRequestDto request)
        {
            if (!await _access.HasAnyPermissionAsync(User, PermissionKeys.StockInput, PermissionKeys.CustomerSalesReturn))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<List<MajorItemSalesReportLineDto>>.Fail("You do not have permission to view major item sales reports."));
            }

            if (request == null)
            {
                return BadRequest(ApiResponse<List<MajorItemSalesReportLineDto>>.Fail(
                    "Report criteria are required."));
            }

            var rows = await _service.GetMajorItemSalesAsync(request);
            return Ok(ApiResponse<List<MajorItemSalesReportLineDto>>.Ok(rows, "Report loaded."));
        }

        [HttpPost("major-item-report")]
        public async Task<IActionResult> GetMajorItemReport([FromBody] MajorItemReportRequestDto request)
        {
            if (!await _access.HasAnyPermissionAsync(User, PermissionKeys.StockInput, PermissionKeys.CustomerSalesReturn))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<List<MajorItemReportLineDto>>.Fail("You do not have permission to view major item reports."));
            }

            if (request == null)
            {
                return BadRequest(ApiResponse<List<MajorItemReportLineDto>>.Fail(
                    "Report criteria are required."));
            }

            if (request.FromDate.HasValue && request.ToDate.HasValue && request.FromDate > request.ToDate)
            {
                return BadRequest(ApiResponse<List<MajorItemReportLineDto>>.Fail(
                    "Invalid date range."));
            }

            var rows = await _service.GetMajorItemReportAsync(request);
            return Ok(ApiResponse<List<MajorItemReportLineDto>>.Ok(rows, "Report loaded."));
        }

        [HttpPost("price-list-report")]
        public async Task<IActionResult> GetPriceListReport([FromBody] PriceListReportRequestDto request)
        {
            if (!await _access.HasAnyPermissionAsync(User, PermissionKeys.StockInput, PermissionKeys.CustomerSalesReturn))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<List<PriceListReportLineDto>>.Fail("You do not have permission to view price list reports."));
            }

            if (request == null)
            {
                return BadRequest(ApiResponse<List<PriceListReportLineDto>>.Fail(
                    "Report criteria are required."));
            }

            var rows = await _service.GetPriceListReportAsync(request);
            return Ok(ApiResponse<List<PriceListReportLineDto>>.Ok(rows, "Report loaded."));
        }

        [HttpPost("stock-transfer")]
        public async Task<IActionResult> GetStockTransferReport([FromBody] StockTransferReportRequestDto request)
        {
            if (!await _access.HasAnyPermissionAsync(User, PermissionKeys.StockInput, PermissionKeys.CustomerSalesReturn))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<List<StockTransferReportLineDto>>.Fail("You do not have permission to view stock transfer reports."));
            }

            if (request == null)
            {
                return BadRequest(ApiResponse<List<StockTransferReportLineDto>>.Fail(
                    "Report criteria are required."));
            }

            if (request.DateMode?.Trim().Equals("DateRange", StringComparison.OrdinalIgnoreCase) == true
                && request.FromDate.HasValue
                && request.ToDate.HasValue
                && request.FromDate > request.ToDate)
            {
                return BadRequest(ApiResponse<List<StockTransferReportLineDto>>.Fail(
                    "Invalid date range."));
            }

            var rows = await _service.GetStockTransferReportAsync(request);
            return Ok(ApiResponse<List<StockTransferReportLineDto>>.Ok(rows, "Report loaded."));
        }

        [HttpPost("layaway-report")]
        public async Task<IActionResult> GetLayawayReport([FromBody] LayawayReportRequestDto request)
        {
            if (!await _access.HasAnyPermissionAsync(User, PermissionKeys.Layaway))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<List<LayawayReportLineDto>>.Fail("You do not have permission to view layaway reports."));
            }

            if (request == null)
            {
                return BadRequest(ApiResponse<List<LayawayReportLineDto>>.Fail(
                    "Report criteria are required."));
            }

            if (request.CustomerMode?.Trim().Equals("One", StringComparison.OrdinalIgnoreCase) == true &&
                string.IsNullOrWhiteSpace(request.CustomerAccNo))
            {
                return BadRequest(ApiResponse<List<LayawayReportLineDto>>.Fail(
                    "Customer account number is required."));
            }

            var rows = await _service.GetLayawayReportAsync(request);
            return Ok(ApiResponse<List<LayawayReportLineDto>>.Ok(rows, "Report loaded."));
        }
    }
}

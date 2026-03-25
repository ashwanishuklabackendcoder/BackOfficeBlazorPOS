using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSAPI.Security;

namespace POSAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/returns")]
    public class ReturnsController : ControllerBase
    {
        private readonly IReturnsService _service;
        private readonly ApiAccessService _access;
        private readonly ILogger<ReturnsController> _logger;

        public ReturnsController(
            IReturnsService service,
            ApiAccessService access,
            ILogger<ReturnsController> logger)
        {
            _service = service;
            _access = access;
            _logger = logger;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessReturn([FromBody] ReturnProcessDto dto)
        {
            if (!await _access.HasAnyPermissionAsync(User, PermissionKeys.CustomerSalesReturn))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<bool>.Fail("You do not have permission to process returns."));
            }

            if (dto == null)
                return BadRequest(ApiResponse<bool>.Fail("Return details are required."));

            if (string.IsNullOrWhiteSpace(dto.InvoiceNo))
                return BadRequest(ApiResponse<bool>.Fail("Invoice number is required."));

            if (dto.Lines == null || dto.Lines.Count == 0)
                return BadRequest(ApiResponse<bool>.Fail("Select at least one item to return."));

            try
            {
                await _service.ProcessReturnAsync(dto);
                return Ok(ApiResponse<bool>.Ok(true, "Return processed successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Return processing failed for invoice {InvoiceNo}", dto.InvoiceNo);
                return BadRequest(ApiResponse<bool>.Fail(
                    string.IsNullOrWhiteSpace(ex.Message)
                        ? "Return could not be processed."
                        : ex.Message));
            }
        }

        [HttpGet("invoice/{invoiceNo}")]
        public async Task<IActionResult> GetInvoiceLines(string invoiceNo)
        {
            if (!await _access.HasAnyPermissionAsync(User, PermissionKeys.CustomerSalesReturn))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<List<PosSaleLineDto>>.Fail("You do not have permission to view returnable sales."));
            }

            if (string.IsNullOrWhiteSpace(invoiceNo))
                return BadRequest(ApiResponse<List<PosSaleLineDto>>.Fail("Invoice number is required."));

            var lines = await _service.GetInvoiceLinesAsync(invoiceNo);
            return Ok(ApiResponse<List<PosSaleLineDto>>.Ok(lines, "Invoice lines loaded."));
        }

        [HttpGet("invoices")]
        public async Task<IActionResult> GetInvoices(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? customerAccNo)
        {
            if (!await _access.HasAnyPermissionAsync(User, PermissionKeys.CustomerSalesReturn))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<List<ReturnInvoiceLookupDto>>.Fail("You do not have permission to view invoices for returns."));
            }

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
            {
                return BadRequest(ApiResponse<List<ReturnInvoiceLookupDto>>.Fail(
                    "From date cannot be later than to date."));
            }

            var invoices = await _service.GetInvoicesAsync(fromDate, toDate, customerAccNo);
            return Ok(ApiResponse<List<ReturnInvoiceLookupDto>>.Ok(invoices, "Invoices loaded."));
        }

        [HttpGet("receipt/{invoiceNo}")]
        public async Task<IActionResult> GetReceipt(string invoiceNo)
        {
            if (!await _access.HasAnyPermissionAsync(User, PermissionKeys.CustomerSalesReturn))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<PosReceiptDto>.Fail("You do not have permission to view receipts."));
            }

            if (string.IsNullOrWhiteSpace(invoiceNo))
                return BadRequest(ApiResponse<PosReceiptDto>.Fail("Invoice number is required."));

            var receipt = await _service.GetReceiptAsync(invoiceNo);
            if (receipt == null)
                return NotFound(ApiResponse<PosReceiptDto>.Fail("Receipt not found."));

            return Ok(ApiResponse<PosReceiptDto>.Ok(receipt, "Receipt loaded."));
        }

        [HttpGet("history/{invoiceNo}")]
        public async Task<IActionResult> GetReturnHistory(string invoiceNo)
        {
            if (!await _access.HasAnyPermissionAsync(User, PermissionKeys.CustomerSalesReturn))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<List<ReturnHistoryDto>>.Fail("You do not have permission to view return history."));
            }

            if (string.IsNullOrWhiteSpace(invoiceNo))
                return BadRequest(ApiResponse<List<ReturnHistoryDto>>.Fail("Invoice number is required."));

            var history = await _service.GetReturnHistoryAsync(invoiceNo);
            return Ok(ApiResponse<List<ReturnHistoryDto>>.Ok(history, "Return history loaded."));
        }
    }
}

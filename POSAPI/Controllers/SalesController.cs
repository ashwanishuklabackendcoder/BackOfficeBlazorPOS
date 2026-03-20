using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSAPI.Security;
using POSAPI.Services;

namespace POSAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/sales")]
    public class SalesController : ControllerBase
    {
        private readonly ISalesService _service;
        private readonly ITerminalOptionRepository _terminalOptions;
        private readonly IPrintJobService _printJobs;
        private readonly IPrintJobDispatcher _dispatcher;
        private readonly IEscPosBuilder _escPos;
        private readonly ApiAccessService _access;
        private readonly ILogger<SalesController> _logger;

        public SalesController(
            ISalesService service,
            ITerminalOptionRepository terminalOptions,
            IPrintJobService printJobs,
            IPrintJobDispatcher dispatcher,
            IEscPosBuilder escPos,
            ApiAccessService access,
            ILogger<SalesController> logger)
        {
            _service = service;
            _terminalOptions = terminalOptions;
            _printJobs = printJobs;
            _dispatcher = dispatcher;
            _escPos = escPos;
            _access = access;
            _logger = logger;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessSale([FromBody] PosSaleRequestDto dto)
        {
            if (!await _access.HasAnyPermissionAsync(User, PermissionKeys.Till))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<SaleProcessResultDto>.Fail("You do not have permission to process sales."));
            }

            if (dto == null)
                return BadRequest(ApiResponse<SaleProcessResultDto>.Fail("Sale details are required."));

            if (dto.Lines == null || dto.Lines.Count == 0)
            {
                return BadRequest(ApiResponse<SaleProcessResultDto>.Fail(
                    "Add at least one item before processing the sale."));
            }

            try
            {
                var invoiceNo = await _service.ProcessSaleAsync(dto);
                var printResult = await TryQueueReceiptPrintAsync(dto, invoiceNo);

                return Ok(ApiResponse<SaleProcessResultDto>.Ok(
                    new SaleProcessResultDto
                    {
                        InvoiceNumber = invoiceNo,
                        ReceiptPrintQueued = printResult.Queued,
                        ReceiptPrintMessage = printResult.Message
                    },
                    "Sale processed successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sale processing failed for terminal {Terminal}", dto.Terminal);
                return BadRequest(ApiResponse<SaleProcessResultDto>.Fail(
                    "Sale could not be completed. Please review the sale and try again."));
            }
        }

        private async Task<(bool Queued, string Message)> TryQueueReceiptPrintAsync(PosSaleRequestDto sale, string invoiceNo)
        {
            try
            {
                var terminalCode = sale.Terminal?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(terminalCode))
                    return (false, "Terminal code missing");

                var assignment = await _terminalOptions.GetByTerminalCodeAsync(terminalCode);
                if (assignment == null || assignment.ReceiptPrinterId <= 0)
                    return (false, $"Receipt printer not assigned for terminal {terminalCode}");

                var payload = _escPos.BuildSaleReceipt(invoiceNo, sale);
                var enqueue = await _printJobs.EnqueueAsync(new PrintJobRequestDto
                {
                    TerminalCode = terminalCode,
                    PrinterConfigId = assignment.ReceiptPrinterId,
                    JobType = "Receipt",
                    Payload = payload
                });

                if (!enqueue.Success || enqueue.Data == null)
                    return (false, enqueue.Message);

                await _dispatcher.DispatchAsync(enqueue.Data);
                return (true, "Receipt queued");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Receipt queue failed for invoice {InvoiceNo}", invoiceNo);
                return (false, "Sale was saved, but receipt queueing failed.");
            }
        }
    }
}

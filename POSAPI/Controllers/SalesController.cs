using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using POSAPI.Services;

namespace POSAPI.Controllers
{
    [ApiController]
    [Route("api/sales")]
    public class SalesController : ControllerBase
    {
        private readonly ISalesService _service;
        private readonly ITerminalOptionRepository _terminalOptions;
        private readonly IPrintJobService _printJobs;
        private readonly IPrintJobDispatcher _dispatcher;
        private readonly IEscPosBuilder _escPos;

        public SalesController(
            ISalesService service,
            ITerminalOptionRepository terminalOptions,
            IPrintJobService printJobs,
            IPrintJobDispatcher dispatcher,
            IEscPosBuilder escPos)
        {
            _service = service;
            _terminalOptions = terminalOptions;
            _printJobs = printJobs;
            _dispatcher = dispatcher;
            _escPos = escPos;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessSale([FromBody] PosSaleRequestDto dto)
        {
            try
            {
                var invoiceNo = await _service.ProcessSaleAsync(dto);
                var printResult = await TryQueueReceiptPrintAsync(dto, invoiceNo);

                return Ok(new
                {
                    InvoiceNumber = invoiceNo,
                    ReceiptPrintQueued = printResult.Queued,
                    ReceiptPrintMessage = printResult.Message
                });
            }
            catch (Exception ex)
            {
                // TEMP: expose error for debugging
                return BadRequest(new { error = ex.Message, stack = ex.StackTrace });
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
                return (false, $"Receipt queue failed: {ex.Message}");
            }
        }
    }
}

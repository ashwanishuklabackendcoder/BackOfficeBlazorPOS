using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace POSAPI.Controllers
{
    [ApiController]
    [Route("api/returns")]
    public class ReturnsController : ControllerBase
    {
        private readonly IReturnsService _service;

        public ReturnsController(IReturnsService service)
        {
            _service = service;
        }
        [HttpPost("process")]
        public async Task<IActionResult> ProcessReturn(ReturnProcessDto dto)
        {
            try
            {
                await _service.ProcessReturnAsync(dto);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Return processed"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = ex.Message
                });
            }
        }

        //[HttpPost("process")]
        //public async Task<IActionResult> Process(ReturnProcessDto dto)
        //{
        //    await _service.ProcessReturnAsync(dto);
        //    return Ok();
        //}
        [HttpGet("invoice/{invoiceNo}")]
        public async Task<IActionResult> GetInvoiceLines(string invoiceNo)
        {
            var lines = await _service.GetInvoiceLinesAsync(invoiceNo);

            return Ok(new ApiResponse<List<PosSaleLineDto>>
            {
                Success = true,
                Data = lines,
                Message = "Invoice lines loaded"
            });
        }

        [HttpGet("invoices")]
        public async Task<IActionResult> GetInvoices(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? customerAccNo)
        {
            var invoices = await _service.GetInvoicesAsync(fromDate, toDate, customerAccNo);

            return Ok(new ApiResponse<List<ReturnInvoiceLookupDto>>
            {
                Success = true,
                Data = invoices,
                Message = "Invoices loaded"
            });
        }

        [HttpGet("receipt/{invoiceNo}")]
        public async Task<IActionResult> GetReceipt(string invoiceNo)
        {
            var receipt = await _service.GetReceiptAsync(invoiceNo);
            if (receipt == null)
            {
                return NotFound(new ApiResponse<PosReceiptDto>
                {
                    Success = false,
                    Data = null,
                    Message = "Receipt not found"
                });
            }

            return Ok(new ApiResponse<PosReceiptDto>
            {
                Success = true,
                Data = receipt,
                Message = "Receipt loaded"
            });
        }


    }

}

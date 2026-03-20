using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POSAPI.Controllers
{
    [Route("product-enquiry")]
    [ApiController]
    [Authorize]
    public class ProductEnquiryController : ControllerBase
    {
        private readonly IProductEnquiryService _service;

        public ProductEnquiryController(IProductEnquiryService service)
        {
            _service = service;
        }

        [HttpGet("header/{partNo}")]
        public async Task<ActionResult<ProductEnquiryHeaderDto>> GetHeader(string partNo)
        {
            if (string.IsNullOrWhiteSpace(partNo))
                return BadRequest("partNo is required");

            var header = await _service.GetProductHeaderAsync(partNo);
            if (header == null)
                return NotFound();

            return Ok(header);
        }

        [HttpGet("history/{partNo}")]
        public async Task<ActionResult<ProductEnquiryHistoryDto>> GetHistory(string partNo)
        {
            if (string.IsNullOrWhiteSpace(partNo))
                return BadRequest("partNo is required");

            var history = await _service.GetProductHistoryAsync(partNo);
            if (history == null)
                return NotFound();

            return Ok(history);
        }

        [HttpGet("location/{partNo}")]
        public async Task<ActionResult<List<ProductEnquiryLocationDto>>> GetLocation(string partNo)
        {
            if (string.IsNullOrWhiteSpace(partNo))
                return BadRequest("partNo is required");

            var data = await _service.GetLocationDistributionAsync(partNo);
            return Ok(data);
        }

        [HttpGet("transfers/{partNo}")]
        public async Task<ActionResult<List<ProductEnquiryTransferDto>>> GetTransfers(string partNo)
        {
            if (string.IsNullOrWhiteSpace(partNo))
                return BadRequest("partNo is required");

            var data = await _service.GetTransfersAsync(partNo);
            return Ok(data);
        }

        [HttpGet("purchase-orders/{partNo}")]
        public async Task<ActionResult<List<ProductEnquiryPurchaseOrderDto>>> GetPurchaseOrders(string partNo)
        {
            if (string.IsNullOrWhiteSpace(partNo))
                return BadRequest("partNo is required");

            var data = await _service.GetPurchaseOrdersAsync(partNo);
            return Ok(data);
        }

        [HttpGet("transactions/{partNo}")]
        public async Task<ActionResult<List<ProductEnquiryTransactionDto>>> GetTransactions(string partNo)
        {
            if (string.IsNullOrWhiteSpace(partNo))
                return BadRequest("partNo is required");

            var data = await _service.GetTransactionsAsync(partNo);
            return Ok(data);
        }

        [HttpGet("sales/{partNo}")]
        public async Task<ActionResult<List<ProductEnquirySaleDto>>> GetSales(string partNo)
        {
            if (string.IsNullOrWhiteSpace(partNo))
                return BadRequest("partNo is required");

            var data = await _service.GetSalesAsync(partNo);
            return Ok(data);
        }

        [HttpGet("stock-check/{partNo}")]
        public async Task<ActionResult<List<ProductEnquiryStockCheckDto>>> GetStockChecks(string partNo)
        {
            if (string.IsNullOrWhiteSpace(partNo))
                return BadRequest("partNo is required");

            var data = await _service.GetStockChecksAsync(partNo);
            return Ok(data);
        }

        [HttpGet("layaways/{partNo}")]
        public async Task<ActionResult<List<ProductEnquiryLayawayDto>>> GetLayaways(string partNo)
        {
            if (string.IsNullOrWhiteSpace(partNo))
                return BadRequest("partNo is required");

            var data = await _service.GetLayawaysAsync(partNo);
            return Ok(data);
        }

        [HttpGet("logs/{partNo}")]
        public async Task<ActionResult<List<ProductEnquiryLogDto>>> GetLogs(string partNo)
        {
            if (string.IsNullOrWhiteSpace(partNo))
                return BadRequest("partNo is required");

            var data = await _service.GetLogsAsync(partNo);
            return Ok(data);
        }

        [HttpGet("internal-orders/{partNo}")]
        public async Task<ActionResult<List<ProductEnquiryInternalOrderDto>>> GetInternalOrders(string partNo)
        {
            if (string.IsNullOrWhiteSpace(partNo))
                return BadRequest("partNo is required");

            var data = await _service.GetInternalOrdersAsync(partNo);
            return Ok(data);
        }
    }
}

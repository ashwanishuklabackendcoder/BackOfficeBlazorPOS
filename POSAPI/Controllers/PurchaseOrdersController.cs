using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POSAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/purchase-orders")]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly IPurchaseOrderService _service;

        public PurchaseOrdersController(IPurchaseOrderService service)
        {
            _service = service;
        }

        [HttpGet("draft/{draftRef:int}")]
        public async Task<IActionResult> GetDraft(int draftRef)
            => Ok(await _service.GetDraftAsync(draftRef));

        [HttpGet("{orderNumber}")]
        public async Task<IActionResult> Get(string orderNumber)
            => Ok(await _service.GetAsync(orderNumber));

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? query, [FromQuery] string? supplierCode, [FromQuery] int? status)
            => Ok(await _service.SearchAsync(query, supplierCode, status));

        [HttpPost("draft")]
        public async Task<IActionResult> SaveDraft([FromBody] PurchaseOrderUpsertRequestDto request)
            => Ok(await _service.SaveDraftAsync(request));

        [HttpPost("raise")]
        public async Task<IActionResult> Raise([FromBody] PurchaseOrderUpsertRequestDto request)
            => Ok(await _service.RaiseAsync(request));

        [HttpPost("direct-raise")]
        public async Task<IActionResult> DirectRaise([FromBody] PurchaseOrderDirectRaiseRequestDto request)
            => Ok(await _service.RaiseDirectAsync(request));

        [HttpPost("receive")]
        public async Task<IActionResult> Receive([FromBody] PurchaseOrderReceiveRequestDto request)
            => Ok(await _service.ReceiveAsync(request));

        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel([FromBody] PurchaseOrderCancelRequestDto request)
            => Ok(await _service.CancelAsync(request.OrderNumber, request.CancelledByCode));
    }

    public class PurchaseOrderCancelRequestDto
    {
        public string OrderNumber { get; set; } = string.Empty;
        public string CancelledByCode { get; set; } = string.Empty;
    }
}

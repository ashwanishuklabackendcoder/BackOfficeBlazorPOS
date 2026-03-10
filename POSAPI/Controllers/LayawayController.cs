using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace POSAPI.Controllers
{
    [ApiController]
    [Route("api/layaway")]
    public class LayawayController : ControllerBase
    {
        private readonly ILayawayService _service;

        public LayawayController(ILayawayService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] LayawayCreateDto request)
        {
            if (string.IsNullOrWhiteSpace(request.CustomerAccNo))
                return BadRequest(ApiResponse<int>.Fail("Customer account is required"));

            if (request.Lines == null || request.Lines.Count == 0)
                return BadRequest(ApiResponse<int>.Fail("No lines provided"));
            try
            {
                var layawayNo = await _service.CreateAsync(request);
                return Ok(ApiResponse<int>.Ok(layawayNo, "Layaway created"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<int>.Fail(ex.Message));
            }
        }
        [HttpGet("active")]
        public async Task<IActionResult> GetActive([FromQuery] string? customerAccNo)
        {
            try
            {
                var data = await _service.GetActiveAsync(customerAccNo);
                return Ok(ApiResponse<List<LayawaySummaryDto>>.Ok(data));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<List<LayawaySummaryDto>>.Fail(ex.Message));
            }
        }

        [HttpGet("{layawayNo:int}")]
        public async Task<IActionResult> Get(int layawayNo)
        {
            try
            {
                var data = await _service.GetAsync(layawayNo);
                if (data == null)
                    return NotFound(ApiResponse<LayawayDetailDto>.Fail("Layaway not found"));

                return Ok(ApiResponse<LayawayDetailDto>.Ok(data));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<LayawayDetailDto>.Fail(ex.Message));
            }
        }

        [HttpPost("{layawayNo:int}/add-line")]
        public async Task<IActionResult> AddLine(int layawayNo, [FromBody] LayawayAddLineDto request)
        {
            try
            {
                await _service.AddLineAsync(layawayNo, request.Line);
                return Ok(ApiResponse<bool>.Ok(true, "Line added"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [HttpPost("{layawayNo:int}/update-line")]
        public async Task<IActionResult> UpdateLine(int layawayNo, [FromBody] LayawayLineUpdateDto request)
        {
            try
            {
                await _service.UpdateLineQtyAsync(layawayNo, request);
                return Ok(ApiResponse<bool>.Ok(true, "Line updated"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [HttpPost("{layawayNo:int}/reverse")]
        public async Task<IActionResult> Reverse(int layawayNo)
        {
            try
            {
                await _service.ReverseAsync(layawayNo);
                return Ok(ApiResponse<bool>.Ok(true, "Layaway reversed"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [HttpPost("{layawayNo:int}/sell")]
        public async Task<IActionResult> Sell(int layawayNo, [FromBody] LayawaySellRequestDto request)
        {
            if (request.LayawayNo == 0)
                request.LayawayNo = layawayNo;
            try
            {
                var invoiceNo = await _service.SellAsync(request);
                return Ok(ApiResponse<string>.Ok(invoiceNo, "Layaway sold"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<string>.Fail(ex.Message));
            }
        }
    }
}

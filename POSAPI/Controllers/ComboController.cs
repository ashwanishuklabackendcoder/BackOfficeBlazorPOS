using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace POSAPI.Controllers
{
    [ApiController]
    [Route("api/combo")]
    public class ComboController : ControllerBase
    {
        private readonly IComboService _service;

        public ComboController(IComboService service)
        {
            _service = service;
        }

        [HttpGet("grid")]
        public async Task<IActionResult> GetGrid()
            => Ok(await _service.GetComboGrid());

        [HttpGet("{comboId:int}")]
        public async Task<IActionResult> GetById(int comboId)
            => Ok(await _service.GetComboById(comboId));

        [HttpGet("partnumber/{comboPartNumber}")]
        public async Task<IActionResult> GetByPartNumber(string comboPartNumber)
            => Ok(await _service.GetActiveComboByPartNumber(comboPartNumber));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ComboSaveRequestDto request)
            => Ok(await _service.CreateCombo(request));

        [HttpPut("{comboId:int}")]
        public async Task<IActionResult> Update(int comboId, [FromBody] ComboSaveRequestDto request)
        {
            request.ComboId = comboId;
            return Ok(await _service.UpdateCombo(request));
        }

        [HttpDelete("{comboId:int}")]
        public async Task<IActionResult> Delete(int comboId)
            => Ok(await _service.SoftDeleteCombo(comboId));
    }
}

using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace POSAPI.Controllers
{
    [ApiController]
    [Route("api/manufacturer")]
    public class ManufacturerController : ControllerBase
    {
        private readonly IManufacturerService _service;

        public ManufacturerController(IManufacturerService service)
        {
            _service = service;
        }
        [HttpGet, Route("GetAllManufacturer")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();
            return Ok(result);
        }
        [HttpGet, Route("GetManufacturers")]
        public async Task<IActionResult> Get(string code)
        {
            var result = await _service.GetAsync(code);
            return Ok(result);
        }

        [HttpPost, Route("SaveManufacturer")]
        public async Task<IActionResult> Save(ManufacturerDto dto)
        {
            var result = await _service.SaveAsync(dto);
            return Ok(result);
        }

        [HttpDelete, Route("DeleteManufacturer")]
        public async Task<IActionResult> Delete(string code)
        {
            var result = await _service.DeleteAsync(code);
            return Ok(result);
        }
        [HttpGet("SuggestMakes")]
        public async Task<IActionResult> SuggestMakes(string term)
        {
            var data = await _service.SuggestMakes(term);
            return Ok(data);
        }

    }
}

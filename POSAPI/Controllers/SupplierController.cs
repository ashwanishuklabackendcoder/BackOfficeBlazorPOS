using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace POSAPI.Controllers
{
    [ApiController]
    [Route("api/supplier")]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _service;

        public SupplierController(ISupplierService service)
        {
            _service = service;
        }
        [HttpGet, Route("GetAllSuppliers")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();
            return Ok(result);
        }
        // GET api/supplier/GetSuppliers?accountNo=123456
        [HttpGet("GetSuppliers")]
        public async Task<IActionResult> Get(string accountNo)
        {
            var result = await _service.GetAsync(accountNo);
            return Ok(result);
        }

        // POST api/supplier/SaveSupplier
        [HttpPost("SaveSupplier")]
        public async Task<IActionResult> Save([FromBody] SupplierDto dto)
        {
            var result = await _service.SaveAsync(dto);
            return Ok(result);
        }

        // DELETE api/supplier/DeleteSupplier?accountNo=123456
        [HttpDelete("DeleteSupplier")]
        public async Task<IActionResult> Delete(string accountNo)
        {
            var result = await _service.DeleteAsync(accountNo);
            return Ok(result);
        }

        [HttpGet("SuggestSuppliers")]
        public async Task<IActionResult> SuggestSuppliers(string term)
        {
            var data = await _service.SuggestSuppliers(term);
            return Ok(data);
        }

    }

}

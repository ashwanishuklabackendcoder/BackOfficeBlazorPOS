using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace POSAPI.Controllers
{
    [ApiController]
    [Route("api/customer")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _service;
        public CustomerController(ICustomerService service)
        {
            _service = service;
        }

        [HttpGet("GetCustomers")]
        public async Task<IActionResult> Get(string accNo)
        {
            return Ok(await _service.GetAsync(accNo));
        }
        [HttpGet("GetAllCustomers")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }
        [HttpPost("SaveCustomer")]
  
        public async Task<ActionResult<ApiResponse<CustomerDto>>> Save(CustomerDto dto)
        {
            try
            {
                var result = await _service.SaveAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<CustomerDto>.Fail(ex.Message));
            }
        }


        [HttpDelete("DeleteCustomer")]
        public async Task<IActionResult> Delete(string accNo)
        {
            return Ok(await _service.DeleteAsync(accNo));
        }
    }
}

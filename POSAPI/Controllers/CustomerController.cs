using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSAPI.Security;

namespace POSAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/customer")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _service;
        private readonly ApiAccessService _access;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(
            ICustomerService service,
            ApiAccessService access,
            ILogger<CustomerController> logger)
        {
            _service = service;
            _access = access;
            _logger = logger;
        }

        [HttpGet("GetCustomers")]
        public async Task<IActionResult> Get(string accNo)
        {
            if (!await _access.HasAnyPermissionAsync(User, PermissionKeys.Customer, PermissionKeys.Till))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<CustomerDto>.Fail("You do not have permission to view customers."));
            }

            if (string.IsNullOrWhiteSpace(accNo))
                return BadRequest(ApiResponse<CustomerDto>.Fail("Customer account number is required."));

            return Ok(await _service.GetAsync(accNo));
        }

        [HttpGet("GetAllCustomers")]
        public async Task<IActionResult> GetAll()
        {
            if (!await _access.HasAnyPermissionAsync(User, PermissionKeys.Customer, PermissionKeys.Till))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<List<CustomerDto>>.Fail("You do not have permission to view customers."));
            }

            return Ok(await _service.GetAllAsync());
        }

        [HttpPost("SearchCustomers")]
        public async Task<IActionResult> Search([FromBody] CustomerSearchRequestDto request)
        {
            if (!await _access.HasAnyPermissionAsync(User, PermissionKeys.Customer, PermissionKeys.Till, PermissionKeys.Layaway))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<List<CustomerDto>>.Fail("You do not have permission to search customers."));
            }

            if (request == null)
                return BadRequest(ApiResponse<List<CustomerDto>>.Fail("Search criteria are required."));

            var result = await _service.SearchAsync(request);
            return Ok(result);
        }

        [HttpPost("SaveCustomer")]
        public async Task<ActionResult<ApiResponse<CustomerDto>>> Save([FromBody] CustomerDto dto)
        {
            if (!await _access.HasAnyPermissionAsync(User, PermissionKeys.Customer))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<CustomerDto>.Fail("You do not have permission to save customers."));
            }

            if (dto == null)
                return BadRequest(ApiResponse<CustomerDto>.Fail("Customer details are required."));

            try
            {
                var result = await _service.SaveAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Customer save failed for account {AccountNo}", dto.AccNo);
                return BadRequest(ApiResponse<CustomerDto>.Fail(
                    "Customer could not be saved. Please review the details and try again."));
            }
        }

        [HttpDelete("DeleteCustomer")]
        public async Task<IActionResult> Delete(string accNo)
        {
            if (!await _access.HasAnyPermissionAsync(User, PermissionKeys.Customer))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail("You do not have permission to delete customers."));
            }

            if (string.IsNullOrWhiteSpace(accNo))
                return BadRequest(ApiResponse<object>.Fail("Customer account number is required."));

            return Ok(await _service.DeleteAsync(accNo));
        }
    }
}

using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSAPI.Security;

namespace POSAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;
        private readonly ApiAccessService _access;

        public DashboardController(IDashboardService service, ApiAccessService access)
        {
            _service = service;
            _access = access;
        }

        [HttpPost]
        public async Task<IActionResult> Get([FromBody] DashboardRequestDto? request)
        {
            if (!_access.IsAdmin(User))
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<DashboardResponseDto>.Fail("Admin access required."));
            }

            var dto = request ?? new DashboardRequestDto();
            var payload = await _service.GetDashboardAsync(dto);
            return Ok(ApiResponse<DashboardResponseDto>.Ok(payload, "Dashboard loaded."));
        }
    }
}

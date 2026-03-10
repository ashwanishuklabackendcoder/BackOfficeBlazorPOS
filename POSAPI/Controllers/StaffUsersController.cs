using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POSAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/staff-users")]
    public class StaffUsersController : ControllerBase
    {
        private readonly IStaffUserService _service;

        public StaffUsersController(IStaffUserService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!IsAdmin())
                return Forbid();

            return Ok(await _service.GetAllAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStaffUserDto dto)
        {
            if (!IsAdmin())
                return Forbid();

            return Ok(await _service.CreateAsync(dto));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateStaffUserDto dto)
        {
            if (!IsAdmin())
                return Forbid();

            return Ok(await _service.UpdateAsync(dto));
        }

        [HttpPut("permissions")]
        public async Task<IActionResult> SetPermissions([FromBody] UpdateStaffUserPermissionsDto dto)
        {
            if (!IsAdmin())
                return Forbid();

            return Ok(await _service.SetPermissionsAsync(dto));
        }

        [HttpPut("permission-keys")]
        public async Task<IActionResult> SetPermissionKeys([FromBody] UpdateStaffUserPermissionKeysDto dto)
        {
            if (!IsAdmin())
                return Forbid();

            return Ok(await _service.SetPermissionKeysAsync(dto));
        }

        [HttpGet("{userId:int}/permissions")]
        public async Task<IActionResult> GetPermissions(int userId)
        {
            if (!IsAdmin())
                return Forbid();

            return Ok(await _service.GetPermissionsAsync(userId));
        }

        [HttpGet("{userId:int}/permission-keys")]
        public async Task<IActionResult> GetPermissionKeys(int userId)
        {
            if (!IsAdmin())
                return Forbid();

            return Ok(await _service.GetPermissionKeysAsync(userId));
        }

        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetStaffUserPasswordDto dto)
        {
            if (!IsAdmin())
                return Forbid();

            return Ok(await _service.ResetPasswordAsync(dto));
        }

        private bool IsAdmin()
            => bool.TryParse(User.FindFirst("is_admin")?.Value, out var isAdmin) && isAdmin;
    }
}

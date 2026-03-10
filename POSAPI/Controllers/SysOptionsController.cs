using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POSAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/sysoptions")]
    public class SysOptionsController : ControllerBase
    {
        private readonly ISysOptionService _service;

        public SysOptionsController(ISysOptionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (!IsAdmin())
                return Forbid();

            return Ok(await _service.GetAsync());
        }

        [HttpPut]
        public async Task<IActionResult> Save([FromBody] SysOptionsDto dto)
        {
            if (!IsAdmin())
                return Forbid();

            return Ok(await _service.SaveAsync(dto));
        }

        private bool IsAdmin()
            => bool.TryParse(User.FindFirst("is_admin")?.Value, out var isAdmin) && isAdmin;
    }
}

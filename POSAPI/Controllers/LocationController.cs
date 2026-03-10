using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace POSAPI.Controllers
{
    [ApiController]
    [Route("api/location")]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _service;

        public LocationController(ILocationService service)
        {
            _service = service;
        }

        [HttpGet, Route("GetAllLocations")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllLocation();
            return Ok(result);
        }

        [HttpGet, Route("Branches")]
        public async Task<IActionResult> GetActiveBranches()
        {
            var result = await _service.GetActiveBranchesAsync();
            return Ok(result);
        }

        [HttpGet, Route("Branch/{id:int}")]
        public async Task<IActionResult> GetBranch(int id)
        {
            var result = await _service.GetBranchAsync(id);
            return Ok(result);
        }

        [HttpPost, Route("Branch")]
        public async Task<IActionResult> SaveBranch([FromBody] BranchDetailDto dto)
        {
            var result = await _service.SaveBranchAsync(dto);
            return Ok(result);
        }
    }
}

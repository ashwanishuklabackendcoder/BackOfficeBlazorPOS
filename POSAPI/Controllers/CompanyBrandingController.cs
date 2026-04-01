using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POSAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/company-branding")]
    public class CompanyBrandingController : ControllerBase
    {
        private readonly ISysOptionService _service;

        public CompanyBrandingController(ISysOptionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var res = await _service.GetAsync();
            if (!res.Success || res.Data == null)
                return Ok(ApiResponse<CompanyBrandingDto>.Fail(res.Message ?? "Unable to load company branding."));

            return Ok(ApiResponse<CompanyBrandingDto>.Ok(new CompanyBrandingDto
            {
                CompanyName = res.Data.CompanyName ?? string.Empty,
                CompanyLogoUrl = res.Data.CompanyLogoUrl ?? string.Empty
            }, "Company branding loaded."));
        }
    }
}

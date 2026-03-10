using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using POSAPI.Services;

namespace POSAPI.Controllers;

[ApiController]
[Route("api/quick-items")]
public class QuickItemsController : ControllerBase
{
    private readonly IQuickShortcutService _service;

    public QuickItemsController(IQuickShortcutService service)
    {
        _service = service;
    }

    [HttpGet("admin")]
    public async Task<IActionResult> GetAdmin()
        => Ok(await _service.GetAdminAsync());

    [HttpPost("admin")]
    public async Task<IActionResult> SaveAdmin([FromBody] List<QuickShortcutItemDto>? items)
        => Ok(await _service.SaveAdminAsync(items ?? new List<QuickShortcutItemDto>()));

    [HttpGet("pos")]
    public async Task<IActionResult> GetPos([FromQuery] string locationCode = "01")
        => Ok(await _service.GetForPosAsync(locationCode));
}

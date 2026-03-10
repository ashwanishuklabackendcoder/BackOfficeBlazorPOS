using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services;

public interface IQuickShortcutService
{
    Task<ApiResponse<List<QuickShortcutItemDto>>> GetAdminAsync();
    Task<ApiResponse<bool>> SaveAdminAsync(List<QuickShortcutItemDto> items);
    Task<ApiResponse<List<QuickShortcutItemDto>>> GetForPosAsync(string locationCode);
}

using System.Net.Http.Json;
using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services;

public class QuickShortcutService : IQuickShortcutService
{
    private readonly HttpClient _http;

    public QuickShortcutService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<List<QuickShortcutItemDto>>> GetAdminAsync()
    {
        return await _http.GetFromJsonAsync<ApiResponse<List<QuickShortcutItemDto>>>("api/quick-items/admin")
            ?? ApiResponse<List<QuickShortcutItemDto>>.Fail("No response from server");
    }

    public async Task<ApiResponse<bool>> SaveAdminAsync(List<QuickShortcutItemDto> items)
    {
        var res = await _http.PostAsJsonAsync("api/quick-items/admin", items);
        return await res.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Invalid server response");
    }

    public async Task<ApiResponse<List<QuickShortcutItemDto>>> GetForPosAsync(string locationCode)
    {
        return await _http.GetFromJsonAsync<ApiResponse<List<QuickShortcutItemDto>>>(
            $"api/quick-items/pos?locationCode={Uri.EscapeDataString(locationCode ?? "01")}")
            ?? ApiResponse<List<QuickShortcutItemDto>>.Fail("No response from server");
    }
}

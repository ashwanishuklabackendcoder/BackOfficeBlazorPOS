using System.Net.Http.Json;
using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly HttpClient _http;

        public DashboardService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ApiResponse<DashboardResponseDto>> GetDashboardAsync(DashboardRequestDto request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/dashboard", request);
                if (!response.IsSuccessStatusCode)
                    return await ApiResponseReader.ReadAsync<DashboardResponseDto>(response, "Dashboard load failed.");

                return await ApiResponseReader.ReadAsync<DashboardResponseDto>(response, "Dashboard load failed.");
            }
            catch
            {
                return ApiResponse<DashboardResponseDto>.Fail("Unable to load dashboard.");
            }
        }
    }
}

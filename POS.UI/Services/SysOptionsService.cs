using System.Net.Http.Json;
using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public class SysOptionsService : ISysOptionsService
    {
        private readonly HttpClient _http;

        public SysOptionsService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ApiResponse<SysOptionsDto>> GetAsync()
        {
            return await _http.GetFromJsonAsync<ApiResponse<SysOptionsDto>>("api/sysoptions")
                   ?? ApiResponse<SysOptionsDto>.Fail("Null response");
        }

        public async Task<ApiResponse<SysOptionsDto>> SaveAsync(SysOptionsDto dto)
        {
            var res = await _http.PutAsJsonAsync("api/sysoptions", dto);
            return await res.Content.ReadFromJsonAsync<ApiResponse<SysOptionsDto>>()
                   ?? ApiResponse<SysOptionsDto>.Fail("Null response");
        }
    }
}

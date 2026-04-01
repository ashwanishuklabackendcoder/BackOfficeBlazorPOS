using System.Net.Http.Json;
using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public class SysOptionsService : ISysOptionsService
    {
        private readonly HttpClient _http;
        public event Action<SysOptionsDto?>? Changed;
        public SysOptionsDto? Current { get; private set; }

        public SysOptionsService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ApiResponse<SysOptionsDto>> GetAsync()
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<SysOptionsDto>>("api/sysoptions")
                           ?? ApiResponse<SysOptionsDto>.Fail("Null response");

            if (response.Success && response.Data != null)
                Current = response.Data;

            return response;
        }

        public async Task<ApiResponse<SysOptionsDto>> SaveAsync(SysOptionsDto dto)
        {
            var res = await _http.PutAsJsonAsync("api/sysoptions", dto);
            var response = await res.Content.ReadFromJsonAsync<ApiResponse<SysOptionsDto>>()
                   ?? ApiResponse<SysOptionsDto>.Fail("Null response");

            if (response.Success && response.Data != null)
            {
                Current = response.Data;
                Changed?.Invoke(Current);
            }

            return response;
        }
    }
}

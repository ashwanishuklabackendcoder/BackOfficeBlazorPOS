using BackOfficeBlazor.Shared.DTOs;
using System.Net.Http.Json;

namespace POS.UI.Services
{
    public class LayawayService : ILayawayService
    {
        private readonly HttpClient _http;

        public LayawayService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ApiResponse<int>> CreateAsync(LayawayCreateDto request)
        {
            var res = await _http.PostAsJsonAsync("api/layaway/create", request);
            return await ReadApiResponseAsync<int>(res);
        }

        public async Task<ApiResponse<List<LayawaySummaryDto>>> GetActiveAsync(string? customerAccNo)
        {
            var url = string.IsNullOrWhiteSpace(customerAccNo)
                ? "api/layaway/active"
                : $"api/layaway/active?customerAccNo={customerAccNo}";

            var res = await _http.GetAsync(url);
            return await ReadApiResponseAsync<List<LayawaySummaryDto>>(res);
        }

        public async Task<ApiResponse<LayawayDetailDto>> GetAsync(int layawayNo)
        {
            var res = await _http.GetAsync($"api/layaway/{layawayNo}");
            return await ReadApiResponseAsync<LayawayDetailDto>(res);
        }

        public async Task<ApiResponse<bool>> AddLineAsync(int layawayNo, LayawayLineDto line)
        {
            var res = await _http.PostAsJsonAsync(
                $"api/layaway/{layawayNo}/add-line", new LayawayAddLineDto { Line = line });

            return await ReadApiResponseAsync<bool>(res);
        }

        public async Task<ApiResponse<bool>> UpdateLineAsync(int layawayNo, LayawayLineUpdateDto update)
        {
            var res = await _http.PostAsJsonAsync(
                $"api/layaway/{layawayNo}/update-line", update);

            return await ReadApiResponseAsync<bool>(res);
        }

        public async Task<ApiResponse<bool>> ReverseAsync(int layawayNo)
        {
            var res = await _http.PostAsJsonAsync(
                $"api/layaway/{layawayNo}/reverse", new { });

            return await ReadApiResponseAsync<bool>(res);
        }

        public async Task<ApiResponse<string>> SellAsync(LayawaySellRequestDto request)
        {
            var res = await _http.PostAsJsonAsync(
                $"api/layaway/{request.LayawayNo}/sell", request);

            return await ReadApiResponseAsync<string>(res);
        }

        private static async Task<ApiResponse<T>> ReadApiResponseAsync<T>(HttpResponseMessage res)
        {
            var raw = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                return ApiResponse<T>.Fail(raw);

            try
            {
                var data = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<T>>(raw,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return data ?? ApiResponse<T>.Fail("Null response");
            }
            catch
            {
                return ApiResponse<T>.Fail(raw);
            }
        }
    }
}

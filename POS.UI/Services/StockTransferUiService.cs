using BackOfficeBlazor.Shared.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace POS.UI.Services
{
    public class StockTransferUiService : IStockTransferUiService
    {
        private readonly HttpClient _http;

        public StockTransferUiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ApiResponse<bool>> ApplyTransferAsync(StockTransferInputDto dto)
        {
            try
            {
                var res = await _http.PostAsJsonAsync(
                    "api/stock-transfer/apply", dto);
                var raw = await res.Content.ReadAsStringAsync();

                if (!res.IsSuccessStatusCode)
                {
                    var message = string.IsNullOrWhiteSpace(raw)
                        ? $"Transfer failed ({(int)res.StatusCode})."
                        : raw;
                    return ApiResponse<bool>.Fail(message);
                }

                if (string.IsNullOrWhiteSpace(raw))
                    return ApiResponse<bool>.Ok(true, "Transfer applied.");

                var parsed = JsonSerializer.Deserialize<ApiResponse<bool>>(raw, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return parsed ?? ApiResponse<bool>.Fail("Invalid server response.");
            }
            catch
            {
                return ApiResponse<bool>.Fail("Unable to process transfer request. Check connection and try again.");
            }
        }
    }

}

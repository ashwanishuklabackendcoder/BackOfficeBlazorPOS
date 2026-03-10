using BackOfficeBlazor.Shared.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace POS.UI.Services
{
    public class ReturnService : IReturnService
    {
        private readonly HttpClient _http;

        public ReturnService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ApiResponse<List<ReturnInvoiceLookupDto>>> GetInvoicesAsync(DateTime? fromDate, DateTime? toDate, string? customerAccNo)
        {
            var query = new List<string>();
            if (fromDate.HasValue)
                query.Add($"fromDate={Uri.EscapeDataString(fromDate.Value.ToString("yyyy-MM-dd"))}");
            if (toDate.HasValue)
                query.Add($"toDate={Uri.EscapeDataString(toDate.Value.ToString("yyyy-MM-dd"))}");
            if (!string.IsNullOrWhiteSpace(customerAccNo))
                query.Add($"customerAccNo={Uri.EscapeDataString(customerAccNo.Trim())}");

            var suffix = query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;
            var url = $"api/returns/invoices{suffix}";

            var res = await _http.GetFromJsonAsync<ApiResponse<List<ReturnInvoiceLookupDto>>>(url);
            return res ?? new ApiResponse<List<ReturnInvoiceLookupDto>>
            {
                Success = false,
                Message = "No response from server",
                Data = new()
            };
        }

        public async Task<ApiResponse<List<PosSaleLineDto>>> GetSaleLinesAsync(string invoiceNo)
        {
            var res = await _http.GetFromJsonAsync<ApiResponse<List<PosSaleLineDto>>>($"api/returns/invoice/{invoiceNo}");

            return res ?? new ApiResponse<List<PosSaleLineDto>>
            {
                Success = false,
                Message = "No response from server",
                Data = new()
            };
        }



        public async Task<ApiResponse<bool>> ProcessReturnAsync(ReturnProcessDto dto)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/returns/process", dto);
                var raw = await res.Content.ReadAsStringAsync();

                if (!res.IsSuccessStatusCode)
                {
                    var message = string.IsNullOrWhiteSpace(raw)
                        ? $"Return request failed ({(int)res.StatusCode})."
                        : raw;
                    return ApiResponse<bool>.Fail(message);
                }

                var parsed = JsonSerializer.Deserialize<ApiResponse<bool>>(raw, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return parsed ?? ApiResponse<bool>.Fail("Invalid server response.");
            }
            catch
            {
                return ApiResponse<bool>.Fail("Unable to process return request. Check connection and try again.");
            }
        }
    }

}

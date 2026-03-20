using BackOfficeBlazor.Shared.DTOs;
using System.Net.Http.Json;

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

            var response = await _http.GetAsync(url);
            var payload = await ApiResponseReader.ReadAsync<List<ReturnInvoiceLookupDto>>(
                response,
                "Unable to load invoices.");

            return response.IsSuccessStatusCode
                ? payload
                : ApiResponse<List<ReturnInvoiceLookupDto>>.Fail(payload.Message);
        }

        public async Task<ApiResponse<List<PosSaleLineDto>>> GetSaleLinesAsync(string invoiceNo)
        {
            var response = await _http.GetAsync($"api/returns/invoice/{invoiceNo}");
            var payload = await ApiResponseReader.ReadAsync<List<PosSaleLineDto>>(
                response,
                "Unable to load invoice lines.");

            return response.IsSuccessStatusCode
                ? payload
                : ApiResponse<List<PosSaleLineDto>>.Fail(payload.Message);
        }

        public async Task<ApiResponse<PosReceiptDto>> GetReceiptAsync(string invoiceNo)
        {
            var response = await _http.GetAsync($"api/returns/receipt/{invoiceNo}");
            var payload = await ApiResponseReader.ReadAsync<PosReceiptDto>(
                response,
                "Unable to load receipt.");

            return response.IsSuccessStatusCode
                ? payload
                : ApiResponse<PosReceiptDto>.Fail(payload.Message);
        }

        public async Task<ApiResponse<List<ReturnHistoryDto>>> GetReturnHistoryAsync(string invoiceNo)
        {
            var response = await _http.GetAsync($"api/returns/history/{invoiceNo}");
            var payload = await ApiResponseReader.ReadAsync<List<ReturnHistoryDto>>(
                response,
                "Unable to load return history.");

            return response.IsSuccessStatusCode
                ? payload
                : ApiResponse<List<ReturnHistoryDto>>.Fail(payload.Message);
        }

        public async Task<ApiResponse<bool>> ProcessReturnAsync(ReturnProcessDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/returns/process", dto);
                var payload = await ApiResponseReader.ReadAsync<bool>(
                    response,
                    "Unable to process return.");

                return response.IsSuccessStatusCode
                    ? payload
                    : ApiResponse<bool>.Fail(payload.Message);
            }
            catch
            {
                return ApiResponse<bool>.Fail("Unable to process return request. Check connection and try again.");
            }
        }
    }
}

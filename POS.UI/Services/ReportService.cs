using BackOfficeBlazor.Shared.DTOs;
using System.Net.Http.Json;

namespace POS.UI.Services
{
    public class ReportService : IReportService
    {
        private readonly HttpClient _http;

        public ReportService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ApiResponse<List<CustomerSalesReturnLineDto>>> GetCustomerSalesReturnsAsync(
            CustomerSalesReturnReportRequestDto request)
        {
            var res = await _http.PostAsJsonAsync("api/reports/customer-sales-returns", request);

            if (!res.IsSuccessStatusCode)
                return ApiResponse<List<CustomerSalesReturnLineDto>>.Fail(
                    await res.Content.ReadAsStringAsync());

            return await res.Content.ReadFromJsonAsync<ApiResponse<List<CustomerSalesReturnLineDto>>>()
                   ?? ApiResponse<List<CustomerSalesReturnLineDto>>.Fail("Null response");
        }
    }
}

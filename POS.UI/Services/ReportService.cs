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

        public async Task<ApiResponse<List<StockPositionLineDto>>> GetStockPositionAsync(
            StockPositionReportRequestDto request)
        {
            var res = await _http.PostAsJsonAsync("api/reports/stock-position", request);

            if (!res.IsSuccessStatusCode)
                return ApiResponse<List<StockPositionLineDto>>.Fail(
                    await res.Content.ReadAsStringAsync());

            return await res.Content.ReadFromJsonAsync<ApiResponse<List<StockPositionLineDto>>>()
                   ?? ApiResponse<List<StockPositionLineDto>>.Fail("Null response");
        }

        public async Task<ApiResponse<List<MajorItemSalesReportLineDto>>> GetMajorItemSalesAsync(
            MajorItemSalesReportRequestDto request)
        {
            var res = await _http.PostAsJsonAsync("api/reports/major-item-sales", request);

            if (!res.IsSuccessStatusCode)
                return ApiResponse<List<MajorItemSalesReportLineDto>>.Fail(
                    await res.Content.ReadAsStringAsync());

            return await res.Content.ReadFromJsonAsync<ApiResponse<List<MajorItemSalesReportLineDto>>>()
                   ?? ApiResponse<List<MajorItemSalesReportLineDto>>.Fail("Null response");
        }

        public async Task<ApiResponse<List<MajorItemReportLineDto>>> GetMajorItemReportAsync(
            MajorItemReportRequestDto request)
        {
            var res = await _http.PostAsJsonAsync("api/reports/major-item-report", request);

            if (!res.IsSuccessStatusCode)
                return ApiResponse<List<MajorItemReportLineDto>>.Fail(
                    await res.Content.ReadAsStringAsync());

            return await res.Content.ReadFromJsonAsync<ApiResponse<List<MajorItemReportLineDto>>>()
                   ?? ApiResponse<List<MajorItemReportLineDto>>.Fail("Null response");
        }

        public async Task<ApiResponse<List<PriceListReportLineDto>>> GetPriceListReportAsync(
            PriceListReportRequestDto request)
        {
            var res = await _http.PostAsJsonAsync("api/reports/price-list-report", request);

            if (!res.IsSuccessStatusCode)
                return ApiResponse<List<PriceListReportLineDto>>.Fail(
                    await res.Content.ReadAsStringAsync());

            return await res.Content.ReadFromJsonAsync<ApiResponse<List<PriceListReportLineDto>>>()
                   ?? ApiResponse<List<PriceListReportLineDto>>.Fail("Null response");
        }

        public async Task<ApiResponse<List<StockTransferReportLineDto>>> GetStockTransferReportAsync(
            StockTransferReportRequestDto request)
        {
            var res = await _http.PostAsJsonAsync("api/reports/stock-transfer", request);

            if (!res.IsSuccessStatusCode)
                return ApiResponse<List<StockTransferReportLineDto>>.Fail(
                    await res.Content.ReadAsStringAsync());

            return await res.Content.ReadFromJsonAsync<ApiResponse<List<StockTransferReportLineDto>>>()
                   ?? ApiResponse<List<StockTransferReportLineDto>>.Fail("Null response");
        }

        public async Task<ApiResponse<List<LayawayReportLineDto>>> GetLayawayReportAsync(
            LayawayReportRequestDto request)
        {
            var res = await _http.PostAsJsonAsync("api/reports/layaway-report", request);

            if (!res.IsSuccessStatusCode)
                return ApiResponse<List<LayawayReportLineDto>>.Fail(
                    await res.Content.ReadAsStringAsync());

            return await res.Content.ReadFromJsonAsync<ApiResponse<List<LayawayReportLineDto>>>()
                   ?? ApiResponse<List<LayawayReportLineDto>>.Fail("Null response");
        }
    }
}

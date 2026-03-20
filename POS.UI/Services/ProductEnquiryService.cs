using BackOfficeBlazor.Shared.DTOs;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace POS.UI.Services
{
    public class ProductEnquiryService : IProductEnquiryService
    {
        private readonly HttpClient _http;

        public ProductEnquiryService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ProductEnquiryHeaderDto?> GetHeaderAsync(string partNumber)
        {
            if (string.IsNullOrWhiteSpace(partNumber))
                return null;

            var response = await _http.GetFromJsonAsync<ProductEnquiryHeaderDto>($"product-enquiry/header/{partNumber}");
            return response;
        }

        public async Task<ProductEnquiryHistoryDto?> GetHistoryAsync(string partNumber)
        {
            if (string.IsNullOrWhiteSpace(partNumber))
                return null;

            var response = await _http.GetFromJsonAsync<ProductEnquiryHistoryDto>($"product-enquiry/history/{partNumber}");
            return response;
        }

        public async Task<List<ProductEnquiryLocationDto>> GetLocationDistributionAsync(string partNumber)
        {
            if (string.IsNullOrWhiteSpace(partNumber))
                return new();

            var response = await _http.GetFromJsonAsync<List<ProductEnquiryLocationDto>>($"product-enquiry/location/{partNumber}");
            return response ?? new List<ProductEnquiryLocationDto>();
        }

        public async Task<List<ProductEnquiryTransferDto>> GetTransfersAsync(string partNumber)
        {
            if (string.IsNullOrWhiteSpace(partNumber))
                return new();

            var response = await _http.GetFromJsonAsync<List<ProductEnquiryTransferDto>>($"product-enquiry/transfers/{partNumber}");
            return response ?? new List<ProductEnquiryTransferDto>();
        }

        public async Task<List<ProductEnquiryPurchaseOrderDto>> GetPurchaseOrdersAsync(string partNumber)
        {
            if (string.IsNullOrWhiteSpace(partNumber))
                return new();

            var response = await _http.GetFromJsonAsync<List<ProductEnquiryPurchaseOrderDto>>($"product-enquiry/purchase-orders/{partNumber}");
            return response ?? new List<ProductEnquiryPurchaseOrderDto>();
        }

        public async Task<List<ProductEnquiryTransactionDto>> GetTransactionsAsync(string partNumber)
        {
            if (string.IsNullOrWhiteSpace(partNumber))
                return new();

            var response = await _http.GetFromJsonAsync<List<ProductEnquiryTransactionDto>>($"product-enquiry/transactions/{partNumber}");
            return response ?? new List<ProductEnquiryTransactionDto>();
        }

        public async Task<List<ProductEnquirySaleDto>> GetSalesAsync(string partNumber)
        {
            if (string.IsNullOrWhiteSpace(partNumber))
                return new();

            var response = await _http.GetFromJsonAsync<List<ProductEnquirySaleDto>>($"product-enquiry/sales/{partNumber}");
            return response ?? new List<ProductEnquirySaleDto>();
        }

        public async Task<List<ProductEnquiryStockCheckDto>> GetStockCheckHistoryAsync(string partNumber)
        {
            if (string.IsNullOrWhiteSpace(partNumber))
                return new();

            var response = await _http.GetFromJsonAsync<List<ProductEnquiryStockCheckDto>>($"product-enquiry/stock-check/{partNumber}");
            return response ?? new List<ProductEnquiryStockCheckDto>();
        }

        public async Task<List<ProductEnquiryLayawayDto>> GetLayawaysAsync(string partNumber)
        {
            if (string.IsNullOrWhiteSpace(partNumber))
                return new();

            var response = await _http.GetFromJsonAsync<List<ProductEnquiryLayawayDto>>($"product-enquiry/layaways/{partNumber}");
            return response ?? new List<ProductEnquiryLayawayDto>();
        }

        public async Task<List<ProductEnquiryLogDto>> GetLogsAsync(string partNumber)
        {
            if (string.IsNullOrWhiteSpace(partNumber))
                return new();

            var response = await _http.GetFromJsonAsync<List<ProductEnquiryLogDto>>($"product-enquiry/logs/{partNumber}");
            return response ?? new List<ProductEnquiryLogDto>();
        }

        public async Task<List<ProductEnquiryInternalOrderDto>> GetInternalOrdersAsync(string partNumber)
        {
            if (string.IsNullOrWhiteSpace(partNumber))
                return new();

            var response = await _http.GetFromJsonAsync<List<ProductEnquiryInternalOrderDto>>($"product-enquiry/internal-orders/{partNumber}");
            return response ?? new List<ProductEnquiryInternalOrderDto>();
        }
    }
}

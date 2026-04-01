using System.Net.Http.Json;
using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public class PurchaseOrderApiService : IPurchaseOrderApiService
    {
        private readonly HttpClient _http;

        public PurchaseOrderApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ApiResponse<PurchaseOrderWorkspaceDto>> SaveDraftAsync(PurchaseOrderUpsertRequestDto request)
        {
            var response = await _http.PostAsJsonAsync("api/purchase-orders/draft", request);
            return await ApiResponseReader.ReadAsync<PurchaseOrderWorkspaceDto>(response, "Draft save failed");
        }

        public async Task<ApiResponse<PurchaseOrderWorkspaceDto>> GetDraftAsync(int draftRef)
        {
            var response = await _http.GetAsync($"api/purchase-orders/draft/{draftRef}");
            return await ApiResponseReader.ReadAsync<PurchaseOrderWorkspaceDto>(response, "Draft load failed");
        }

        public async Task<ApiResponse<PurchaseOrderWorkspaceDto>> GetAsync(string orderNumber)
        {
            var response = await _http.GetAsync($"api/purchase-orders/{Uri.EscapeDataString(orderNumber)}");
            return await ApiResponseReader.ReadAsync<PurchaseOrderWorkspaceDto>(response, "Order load failed");
        }

        public async Task<ApiResponse<List<PurchaseOrderSummaryDto>>> SearchAsync(string? query, string? supplierCode, int? status)
        {
            var url = $"api/purchase-orders/search?query={Uri.EscapeDataString(query ?? string.Empty)}&supplierCode={Uri.EscapeDataString(supplierCode ?? string.Empty)}&status={status?.ToString() ?? string.Empty}";
            var response = await _http.GetAsync(url);
            return await ApiResponseReader.ReadAsync<List<PurchaseOrderSummaryDto>>(response, "Order search failed");
        }

        public async Task<ApiResponse<List<PurchaseOrderSupplierOptionDto>>> GetSupplierOptionsAsync(int? status)
        {
            var url = $"api/purchase-orders/suppliers?status={status?.ToString() ?? string.Empty}";
            var response = await _http.GetAsync(url);
            return await ApiResponseReader.ReadAsync<List<PurchaseOrderSupplierOptionDto>>(response, "Supplier search failed");
        }

        public async Task<ApiResponse<PurchaseOrderWorkspaceDto>> RaiseAsync(PurchaseOrderUpsertRequestDto request)
        {
            var response = await _http.PostAsJsonAsync("api/purchase-orders/raise", request);
            return await ApiResponseReader.ReadAsync<PurchaseOrderWorkspaceDto>(response, "Raise order failed");
        }

        public async Task<ApiResponse<PurchaseOrderWorkspaceDto>> RaiseDirectAsync(PurchaseOrderDirectRaiseRequestDto request)
        {
             var response = await _http.PostAsJsonAsync("api/purchase-orders/direct-raise", request);
            return await ApiResponseReader.ReadAsync<PurchaseOrderWorkspaceDto>(response, "Direct raise failed");
        }

        public async Task<ApiResponse<PurchaseOrderWorkspaceDto>> ReceiveAsync(PurchaseOrderReceiveRequestDto request)
        {
            var response = await _http.PostAsJsonAsync("api/purchase-orders/receive", request);
            return await ApiResponseReader.ReadAsync<PurchaseOrderWorkspaceDto>(response, "Receive order failed");
        }

        public async Task<ApiResponse<PurchaseOrderWorkspaceDto>> AmendAsync(PurchaseOrderAmendRequestDto request)
        {
            var response = await _http.PostAsJsonAsync("api/purchase-orders/amend", request);
            return await ApiResponseReader.ReadAsync<PurchaseOrderWorkspaceDto>(response, "Amend order failed");
        }

        public async Task<ApiResponse<bool>> CancelAsync(string orderNumber, string cancelledByCode)
        {
            var response = await _http.PostAsJsonAsync("api/purchase-orders/cancel", new PurchaseOrderCancelRequestDto
            {
                OrderNumber = orderNumber,
                CancelledByCode = cancelledByCode
            });
            return await ApiResponseReader.ReadAsync<bool>(response, "Cancel order failed");
        }

        private sealed class PurchaseOrderCancelRequestDto
        {
            public string OrderNumber { get; set; } = string.Empty;
            public string CancelledByCode { get; set; } = string.Empty;
        }
    }
}

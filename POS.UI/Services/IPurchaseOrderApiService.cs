using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public interface IPurchaseOrderApiService
    {
        Task<ApiResponse<PurchaseOrderWorkspaceDto>> SaveDraftAsync(PurchaseOrderUpsertRequestDto request);
        Task<ApiResponse<PurchaseOrderWorkspaceDto>> GetDraftAsync(int draftRef);
        Task<ApiResponse<PurchaseOrderWorkspaceDto>> GetAsync(string orderNumber);
        Task<ApiResponse<List<PurchaseOrderSummaryDto>>> SearchAsync(string? query, string? supplierCode, int? status);
        Task<ApiResponse<List<PurchaseOrderSupplierOptionDto>>> GetSupplierOptionsAsync(int? status);
        Task<ApiResponse<PurchaseOrderWorkspaceDto>> RaiseAsync(PurchaseOrderUpsertRequestDto request);
        Task<ApiResponse<PurchaseOrderWorkspaceDto>> RaiseDirectAsync(PurchaseOrderDirectRaiseRequestDto request);
        Task<ApiResponse<PurchaseOrderWorkspaceDto>> ReceiveAsync(PurchaseOrderReceiveRequestDto request);
        Task<ApiResponse<PurchaseOrderWorkspaceDto>> AmendAsync(PurchaseOrderAmendRequestDto request);
        Task<ApiResponse<bool>> CancelAsync(string orderNumber, string cancelledByCode);
    }
}

using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public interface IReturnService
    {
        Task<ApiResponse<List<ReturnInvoiceLookupDto>>> GetInvoicesAsync(DateTime? fromDate, DateTime? toDate, string? customerAccNo);

        Task<ApiResponse<List<PosSaleLineDto>>> GetSaleLinesAsync(string invoiceNo);

        Task<ApiResponse<bool>> ProcessReturnAsync(ReturnProcessDto dto);
    }

}

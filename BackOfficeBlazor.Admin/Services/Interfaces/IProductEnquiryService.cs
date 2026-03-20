using BackOfficeBlazor.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Interfaces
{
    public interface IProductEnquiryService
    {
        Task<ProductEnquiryHeaderDto?> GetProductHeaderAsync(string partNumber);
        Task<ProductEnquiryHistoryDto?> GetProductHistoryAsync(string partNumber);
        Task<List<ProductEnquiryLocationDto>> GetLocationDistributionAsync(string partNumber);
        Task<List<ProductEnquiryTransferDto>> GetTransfersAsync(string partNumber);
        Task<List<ProductEnquiryPurchaseOrderDto>> GetPurchaseOrdersAsync(string partNumber);
        Task<List<ProductEnquiryTransactionDto>> GetTransactionsAsync(string partNumber);
        Task<List<ProductEnquirySaleDto>> GetSalesAsync(string partNumber);
        Task<List<ProductEnquiryStockCheckDto>> GetStockChecksAsync(string partNumber);
        Task<List<ProductEnquiryLayawayDto>> GetLayawaysAsync(string partNumber);
        Task<List<ProductEnquiryLogDto>> GetLogsAsync(string partNumber);
        Task<List<ProductEnquiryInternalOrderDto>> GetInternalOrdersAsync(string partNumber);
    }
}

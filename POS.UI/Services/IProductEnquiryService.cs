using BackOfficeBlazor.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POS.UI.Services
{
    public interface IProductEnquiryService
    {
        Task<ProductEnquiryHeaderDto?> GetHeaderAsync(string partNumber);
        Task<ProductEnquiryHistoryDto?> GetHistoryAsync(string partNumber);
        Task<List<ProductEnquiryLocationDto>> GetLocationDistributionAsync(string partNumber);
        Task<List<ProductEnquiryTransferDto>> GetTransfersAsync(string partNumber);
        Task<List<ProductEnquiryPurchaseOrderDto>> GetPurchaseOrdersAsync(string partNumber);
        Task<List<ProductEnquiryTransactionDto>> GetTransactionsAsync(string partNumber);
        Task<List<ProductEnquirySaleDto>> GetSalesAsync(string partNumber);
        Task<List<ProductEnquiryStockCheckDto>> GetStockCheckHistoryAsync(string partNumber);
        Task<List<ProductEnquiryLayawayDto>> GetLayawaysAsync(string partNumber);
        Task<List<ProductEnquiryLogDto>> GetLogsAsync(string partNumber);
        Task<List<ProductEnquiryInternalOrderDto>> GetInternalOrdersAsync(string partNumber);
    }
}

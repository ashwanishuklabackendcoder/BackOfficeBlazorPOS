using BackOfficeBlazor.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POS.UI.Services
{
    public interface IReportService
    {
        Task<ApiResponse<List<CustomerSalesReturnLineDto>>> GetCustomerSalesReturnsAsync(
            CustomerSalesReturnReportRequestDto request);

        Task<ApiResponse<List<StockPositionLineDto>>> GetStockPositionAsync(
            StockPositionReportRequestDto request);

        Task<ApiResponse<List<MajorItemSalesReportLineDto>>> GetMajorItemSalesAsync(
            MajorItemSalesReportRequestDto request);

        Task<ApiResponse<List<MajorItemReportLineDto>>> GetMajorItemReportAsync(
            MajorItemReportRequestDto request);

        Task<ApiResponse<List<StockTransferReportLineDto>>> GetStockTransferReportAsync(
            StockTransferReportRequestDto request);

        Task<ApiResponse<List<LayawayReportLineDto>>> GetLayawayReportAsync(
            LayawayReportRequestDto request);
    }
}

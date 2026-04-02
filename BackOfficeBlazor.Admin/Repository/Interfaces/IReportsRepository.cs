using BackOfficeBlazor.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface IReportsRepository
    {
        Task<List<CustomerSalesReturnLineDto>> GetCustomerSalesReturnsAsync(CustomerSalesReturnReportRequestDto request);
        Task<List<StockPositionLineDto>> GetStockPositionAsync(StockPositionReportRequestDto request);
        Task<List<MajorItemSalesReportLineDto>> GetMajorItemSalesAsync(MajorItemSalesReportRequestDto request);
        Task<List<MajorItemReportLineDto>> GetMajorItemReportAsync(MajorItemReportRequestDto request);
        Task<List<PriceListReportLineDto>> GetPriceListReportAsync(PriceListReportRequestDto request);
        Task<List<StockTransferReportLineDto>> GetStockTransferReportAsync(StockTransferReportRequestDto request);
        Task<List<LayawayReportLineDto>> GetLayawayReportAsync(LayawayReportRequestDto request);
    }
}

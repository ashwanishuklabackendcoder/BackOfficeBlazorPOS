using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    public class ReportsService : IReportsService
    {
        private readonly IReportsRepository _repo;

        public ReportsService(IReportsRepository repo)
        {
            _repo = repo;
        }

        public Task<List<CustomerSalesReturnLineDto>> GetCustomerSalesReturnsAsync(
            CustomerSalesReturnReportRequestDto request)
        {
            return _repo.GetCustomerSalesReturnsAsync(request);
        }

        public Task<List<StockPositionLineDto>> GetStockPositionAsync(StockPositionReportRequestDto request)
        {
            return _repo.GetStockPositionAsync(request);
        }

        public Task<List<MajorItemSalesReportLineDto>> GetMajorItemSalesAsync(MajorItemSalesReportRequestDto request)
        {
            return _repo.GetMajorItemSalesAsync(request);
        }

        public Task<List<MajorItemReportLineDto>> GetMajorItemReportAsync(MajorItemReportRequestDto request)
        {
            return _repo.GetMajorItemReportAsync(request);
        }

        public Task<List<PriceListReportLineDto>> GetPriceListReportAsync(PriceListReportRequestDto request)
        {
            return _repo.GetPriceListReportAsync(request);
        }

        public Task<List<StockTransferReportLineDto>> GetStockTransferReportAsync(StockTransferReportRequestDto request)
        {
            return _repo.GetStockTransferReportAsync(request);
        }

        public Task<List<LayawayReportLineDto>> GetLayawayReportAsync(LayawayReportRequestDto request)
        {
            return _repo.GetLayawayReportAsync(request);
        }
    }
}

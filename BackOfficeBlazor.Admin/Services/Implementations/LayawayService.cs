using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    public class LayawayService : ILayawayService
    {
        private readonly ILayawayRepository _repo;
        private readonly ISalesRepository _salesRepo;
        private readonly BackOfficeAdminContext _context;

        public LayawayService(
            ILayawayRepository repo,
            ISalesRepository salesRepo,
            BackOfficeAdminContext context)
        {
            _repo = repo;
            _salesRepo = salesRepo;
            _context = context;
        }

        public Task<int> CreateAsync(LayawayCreateDto request)
            => _repo.CreateAsync(request);

        public Task<System.Collections.Generic.List<LayawaySummaryDto>> GetActiveAsync(string? customerAccNo)
            => _repo.GetActiveAsync(customerAccNo);

        public Task<LayawayDetailDto?> GetAsync(int layawayNo)
            => _repo.GetAsync(layawayNo);

        public Task AddLineAsync(int layawayNo, LayawayLineDto line)
            => _repo.AddLineAsync(layawayNo, line);

        public Task UpdateLineQtyAsync(int layawayNo, LayawayLineUpdateDto update)
            => _repo.UpdateLineQtyAsync(layawayNo, update);

        public Task ReverseAsync(int layawayNo)
            => _repo.ReverseAsync(layawayNo);

        public async Task<string> SellAsync(LayawaySellRequestDto request)
        {
            var detail = await _repo.GetAsync(request.LayawayNo);
            if (detail == null || detail.Status != "A")
                throw new Exception("Layaway not found or not active");

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var invoiceNo = await _salesRepo.GenerateNextInvoiceNumberAsync(
                    detail.Location, detail.Terminal);

                var sale = new PosSaleRequestDto
                {
                    Location = detail.Location,
                    Terminal = detail.Terminal,
                    SalesPerson = detail.SalesPerson,
                    Customer = detail.CustomerAccNo,
                    SubTotal = detail.Lines.Sum(x => x.Net),
                    VatAmount = detail.Lines.Sum(x => x.Vat),
                    NetTotal = detail.Lines.Sum(x => x.Net),
                    Payment = request.Payment,
                    Lines = detail.Lines.Select(x => new PosSaleLineDto
                    {
                        PartNumber = x.PartNumber,
                        StockNo = x.StockNo ?? "",
                        Quantity = x.Quantity,
                        Cost = x.Cost,
                        Sell = x.Sell,
                        Vat = x.Vat,
                        Net = x.Net,
                        IsMajor = x.IsMajor
                    }).ToList()
                };

                foreach (var line in sale.Lines)
                {
                    await _salesRepo.InsertFtt05Async(invoiceNo, sale, line);
                }

                await _salesRepo.InsertFtt11Async(invoiceNo, sale);

                await _repo.MarkStatusAsync(request.LayawayNo, 1, false);

                await tx.CommitAsync();

                return invoiceNo;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}

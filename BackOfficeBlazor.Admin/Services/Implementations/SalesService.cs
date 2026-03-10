using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    public class SalesService : ISalesService
    {
        private readonly ISalesRepository _repo;
        private readonly BackOfficeAdminContext _context;

        public SalesService(ISalesRepository repo, BackOfficeAdminContext context)
        {
            _repo = repo;
            _context = context;
        }

        //public async Task<string> ProcessSaleAsync(PosSaleRequestDto request)
        //{
        //    // 1. Generate Invoice Number
        //    var invoiceNo = await _repo.GenerateNextInvoiceNumberAsync(
        //        request.Location, request.Terminal);

        //    // 2. Insert FTT05 (one row per item)
        //    foreach (var line in request.Lines)
        //    {
        //        await _repo.InsertFtt05Async(invoiceNo, request, line);
        //    }

        //    // 3. Insert FTT11 (one row per invoice)
        //    await _repo.InsertFtt11Async(invoiceNo, request);

        //    // 4. Update Stock
        //    foreach (var line in request.Lines)
        //    {
        //        await _repo.UpdateStockAfterSaleAsync(
        //            line.PartNumber,
        //            line.StockNo,
        //            request.Location,
        //            line.Quantity);
        //    }

        //    return invoiceNo;
        //}

        public async Task<string> ProcessSaleAsync(PosSaleRequestDto request)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Generate Invoice Number
                var invoiceNo = await _repo.GenerateNextInvoiceNumberAsync(
                    request.Location, request.Terminal);

                // 2. Insert FTT05 (one row per item)
                foreach (var line in request.Lines)
                {
                    await _repo.InsertFtt05Async(invoiceNo, request, line);
                }

                // 3. Insert FTT11 (one row per invoice)
                await _repo.InsertFtt11Async(invoiceNo, request);

                // 4. Update Stock + StockLevels
                foreach (var line in request.Lines)
                {
                    await _repo.UpdateStockAfterSaleAsync(
                        line.PartNumber,
                        line.StockNo,
                        request.Location,
                        line.Quantity);
                }

                await tx.CommitAsync();

                return invoiceNo;
            }
            catch
            {
                await tx.RollbackAsync();
                throw; // will become HTTP 500 so UI sees failure
            }
        }

    }

}

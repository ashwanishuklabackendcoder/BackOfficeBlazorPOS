using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class ReportsRepository : IReportsRepository
    {
        private readonly BackOfficeAdminContext _context;

        public ReportsRepository(BackOfficeAdminContext context)
        {
            _context = context;
        }

        public async Task<List<CustomerSalesReturnLineDto>> GetCustomerSalesReturnsAsync(
            CustomerSalesReturnReportRequestDto request)
        {
            var from = request.From;
            var to = request.To;

            var query = _context.FTT05.AsNoTracking().AsQueryable();

            query = query.Where(x =>
                x.DateAndTime != null &&
                x.DateAndTime >= from &&
                x.DateAndTime <= to);

            if (!string.IsNullOrWhiteSpace(request.CustomerAccNo))
                query = query.Where(x => x.Customer == request.CustomerAccNo);

            if (request.Locations != null && request.Locations.Count > 0)
                query = query.Where(x => request.Locations.Contains(x.Location));

            return await query
                .OrderBy(x => x.DateAndTime)
                .Select(x => new CustomerSalesReturnLineDto
                {
                    DateAndTime = x.DateAndTime ?? DateTime.MinValue,
                    InvoiceNumber = x.InvoiceNumber,
                    PartNumber = x.PartNumber,
                    StockNo = x.StockNo,
                    Quantity = x.Quantity,
                    Sell = x.Sell,
                    Net = x.Net,
                    Vat = x.VAT,
                    InOut = x.InOut,
                    Location = x.Location,
                    Terminal = x.Terminal,
                    Customer = x.Customer
                })
                .ToListAsync();
        }
    }
}

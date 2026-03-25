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

            var types = request.TransactionTypes?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList();

            if (types != null && types.Count > 0)
                query = query.Where(x => types.Contains(x.InOut));

            var productItems = _context.ProductItems.AsNoTracking();
            var manufacturers = _context._Makes.AsNoTracking();

            return await (from sale in query
                          join product in productItems on sale.PartNumber equals product.PartNumber into productGroup
                          from product in productGroup.DefaultIfEmpty()
                          join make in manufacturers on product.MakeCode equals make.Code into makeGroup
                          from make in makeGroup.DefaultIfEmpty()
                          orderby sale.DateAndTime
                          select new CustomerSalesReturnLineDto
                          {
                              DateAndTime = sale.DateAndTime ?? DateTime.MinValue,
                              InvoiceNumber = sale.InvoiceNumber,
                              PartNumber = sale.PartNumber,
                              StockNo = sale.StockNo,
                              Description = sale.Description,
                              Manufacturer = make != null
                                  ? make.Name
                                  : product.Make ?? string.Empty,
                              Brand = sale.Band ?? string.Empty,
                              Quantity = sale.Quantity,
                              Sell = sale.Sell,
                              Net = sale.Net,
                              Vat = sale.VAT,
                              InOut = sale.InOut,
                              Location = sale.Location,
                              Terminal = sale.Terminal,
                              Customer = sale.Customer
                          })
                         .ToListAsync();
        }
    }
}

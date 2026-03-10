using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class ReturnsRepository : IReturnsRepository
    {
        private readonly BackOfficeAdminContext _context;

        public ReturnsRepository(BackOfficeAdminContext context)
        {
            _context = context;
        }

        public async Task<List<ReturnInvoiceLookupDto>> GetInvoicesAsync(DateTime? fromDate, DateTime? toDate, string? customerAccNo)
        {
            var query = _context.FTT05
                .AsNoTracking()
                .Where(x => x.InOut == "O");

            var normalizedFrom = fromDate;
            var normalizedTo = toDate;

            if (normalizedFrom.HasValue && !normalizedTo.HasValue)
                normalizedTo = normalizedFrom;
            else if (!normalizedFrom.HasValue && normalizedTo.HasValue)
                normalizedFrom = normalizedTo;

            if (normalizedFrom.HasValue && normalizedTo.HasValue)
            {
                var start = normalizedFrom.Value.Date;
                var end = normalizedTo.Value.Date;
                if (end < start)
                {
                    (start, end) = (end, start);
                }

                var endExclusive = end.AddDays(1);
                query = query.Where(x => x.DateAndTime >= start && x.DateAndTime < endExclusive);
            }

            if (!string.IsNullOrWhiteSpace(customerAccNo))
            {
                var customer = customerAccNo.Trim();
                query = query.Where(x => x.Customer == customer);
            }

            return await query
                .GroupBy(x => x.InvoiceNumber)
                .Select(g => new ReturnInvoiceLookupDto
                {
                    InvoiceNo = g.Key,
                    InvoiceDate = g.Max(x => x.DateAndTime),
                    CustomerAccNo = g
                        .Where(x => !string.IsNullOrWhiteSpace(x.Customer))
                        .Select(x => x.Customer)
                        .FirstOrDefault() ?? string.Empty
                })
                .OrderByDescending(x => x.InvoiceDate)
                .Take(300)
                .ToListAsync();
        }

        public async Task<List<PosSaleLineDto>> GetInvoiceLinesAsync(string invoiceNo)
        {
            var rows = await _context.FTT05
                .Where(x => x.InvoiceNumber == invoiceNo && x.InOut == "O") // Only sales
                .ToListAsync();

            if (rows == null || rows.Count == 0)
                return new List<PosSaleLineDto>();

            // Check if any PartNumber exists
            var rowsWithPartNumber = rows
                .Where(x => !string.IsNullOrWhiteSpace(x.PartNumber))
                .ToList();

            // If any PartNumber rows exist → use only those
            var finalRows = rowsWithPartNumber.Any() ? rowsWithPartNumber : rows;

            var makeLookup = await LoadMakeLookupAsync(finalRows);

            return finalRows.Select(x => new PosSaleLineDto
            {
                PartNumber = x.PartNumber,
                StockNo = x.StockNo,
                Quantity = x.Quantity,
                Terminal = x.Terminal,
                Location = x.Location,
                Sell = x.Sell,
                Net = x.Net,
                Cost = x.Cost,
                Vat = x.VAT,
                IsMajor = !string.IsNullOrEmpty(x.StockNo),
                Make = GetMake(makeLookup, x.PartNumber)
            }).ToList();
        }

        public async Task<PosReceiptDto?> GetReceiptAsync(string invoiceNo)
        {
            var rows = await _context.FTT05
                .Where(x => x.InvoiceNumber == invoiceNo && x.InOut == "O")
                .ToListAsync();

            if (rows == null || rows.Count == 0)
                return null;

            var rowsWithPartNumber = rows
                .Where(x => !string.IsNullOrWhiteSpace(x.PartNumber))
                .ToList();

            var finalRows = rowsWithPartNumber.Any() ? rowsWithPartNumber : rows;
            var header = rows
                .OrderBy(x => x.DateAndTime ?? DateTime.MinValue)
                .First();

            var payments = await _context.FTT11
                .Where(x => x.InvoiceNumber == invoiceNo)
                .ToListAsync();

            var payment = new PosPaymentDto
            {
                Cash = payments.Sum(x => x.Cash),
                Cheque = payments.Sum(x => x.Cheque),
                Credit = payments.Sum(x => x.Credit),
                MasterCard = payments.Sum(x => x.Type3),
                Visa = payments.Sum(x => x.Type4)
            };

            var makeLookup = await LoadMakeLookupAsync(finalRows);

            var lines = finalRows.Select(x => new PosSaleLineDto
            {
                PartNumber = x.PartNumber,
                StockNo = x.StockNo,
                Quantity = x.Quantity,
                Terminal = x.Terminal,
                Location = x.Location,
                Sell = x.Sell,
                Net = x.Net,
                Cost = x.Cost,
                Vat = x.VAT,
                IsMajor = !string.IsNullOrEmpty(x.StockNo),
                Make = GetMake(makeLookup, x.PartNumber)
            }).ToList();

            var subTotal = lines.Sum(x => x.Sell * x.Quantity);
            var vatAmount = lines.Sum(x => x.Vat);
            var netTotal = lines.Sum(x => x.Net);

            return new PosReceiptDto
            {
                InvoiceNumber = invoiceNo,
                Location = header.Location,
                Terminal = header.Terminal,
                Customer = header.Customer,
                SalesPerson = header.SalesPerson,
                ReceiptDateTime = header.DateAndTime,
                Lines = lines,
                SubTotal = subTotal,
                VatAmount = vatAmount,
                NetTotal = netTotal,
                Payment = payment
            };
        }

        private async Task<Dictionary<string, string>> LoadMakeLookupAsync(List<FTT05> rows)
        {
            var partNumbers = rows
                .Select(x => x.PartNumber)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            if (partNumbers.Count == 0)
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var makes = await _context.ProductItems
                .Where(x => partNumbers.Contains(x.PartNumber))
                .Select(x => new { x.PartNumber, x.Make })
                .ToListAsync();

            return makes
                .Where(x => !string.IsNullOrWhiteSpace(x.PartNumber))
                .ToDictionary(
                    x => x.PartNumber,
                    x => x.Make ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase);
        }

        private static string? GetMake(Dictionary<string, string> lookup, string? partNumber)
        {
            if (string.IsNullOrWhiteSpace(partNumber))
                return null;

            return lookup.TryGetValue(partNumber, out var make) ? make : null;
        }

        //public async Task<List<PosSaleLineDto>> GetInvoiceLinesAsync(string invoiceNo)
        //{
        //    var rows = await _context.FTT05
        //        .Where(x => x.InvoiceNumber == invoiceNo && x.InOut == "O") // Only sales
        //        .ToListAsync();

        //    return rows.Select(x => new PosSaleLineDto
        //    {
        //        PartNumber = x.PartNumber,
        //        StockNo = x.StockNo,
        //        Quantity = x.Quantity,
        //        Terminal=x.Terminal,
        //        Location=x.Location,
        //        Sell = x.Sell,
        //        Net = x.Net,
        //        Cost = x.Cost,
        //        Vat = x.VAT,
        //        IsMajor = !string.IsNullOrEmpty(x.StockNo)
        //    }).ToList();
        //}

        public async Task ProcessReturnAsync(ReturnProcessDto dto)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var customer = await GetCustomerByInvoiceAsync(dto.InvoiceNo);

                foreach (var line in dto.Lines)
                {
                    await InsertReturnFtt05(dto, line, customer);
                    await ReverseStock(line, line.Terminal);
                }

                await InsertRefundFtt11(dto);

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        private async Task InsertReturnFtt05(ReturnProcessDto header, ReturnLineDto line, string customer)
        {
            var entity = new FTT05
            {
                DateAndTime = DateTime.Now,
                Date = DateTime.Now.ToString("yyyyMMdd"),
                Time = DateTime.Now.ToString("HHmmss"),

                Location = line.Location,
                InvoiceNumber = header.InvoiceNo,

                PartNumber = line.PartNumber,
                StockNo = line.StockNo ?? "",

                Quantity = -line.Qty,   // 🔥 NEGATIVE
                Sell = -line.Sell,
                Net = -line.Sell,

                InOut = "R",   // Return
                Source = "RE",

                SalesPerson = header.Staff,
                Customer = customer,
                Terminal = line.Terminal
            };

            _context.FTT05.Add(entity);
            await _context.SaveChangesAsync();
        }
        private async Task<string> GetCustomerByInvoiceAsync(string invoiceNo)
        {
            var customer = await _context.FTT05
                .Where(x => x.InvoiceNumber == invoiceNo && x.InOut == "O")
                .Select(x => x.Customer)
                .FirstOrDefaultAsync();

            return customer ?? "";
        }
        private async Task ReverseStock(ReturnLineDto line, string location)
        {
            if (string.IsNullOrEmpty(line.StockNo))
            {
                var stock = await _context._ProductsStock
                    .FirstOrDefaultAsync(x =>
                        x.PartNumber == line.PartNumber &&
                        x.LocationCode == line.Terminal);

                if (stock != null)
                    stock.Quantity += line.Qty;

                await IncrementStockLevel(line.PartNumber, line.Terminal, line.Qty);
            }
            else
            {
                var serial = await _context._ProductsStock
                    .FirstOrDefaultAsync(x =>
                        x.PartNumber == line.PartNumber &&
                        x.StockNumber == line.StockNo &&
                        x.LocationCode == location);

                if (serial != null)
                    serial.IsAvailable = true;
            }

            await _context.SaveChangesAsync();
        }
        private async Task IncrementStockLevel(string partNumber, string location, int qty)
        {
            var level = await _context._ProductsStockLevels
                .FirstAsync(x => x.PartNumber == partNumber);

            var prop = typeof(StockLevels)
                .GetProperty($"L{location}");

            var current = (int)prop!.GetValue(level)!;
            prop.SetValue(level, current + qty);
          
            level.DateUpdated = DateTime.UtcNow;
        }
        private async Task InsertRefundFtt11(ReturnProcessDto dto)
        {
            var entity = new FTT11
            {
                Location = dto.Location,
                InvoiceNumber = dto.InvoiceNo,

                Amount = -(dto.RefundCash + dto.RefundCard),

                Cash = -dto.RefundCash,

                Type3Description = "Card Refund",
                Type3 = -dto.RefundCard
            };

            _context.FTT11.Add(entity);
            await _context.SaveChangesAsync();
        }
    }

}


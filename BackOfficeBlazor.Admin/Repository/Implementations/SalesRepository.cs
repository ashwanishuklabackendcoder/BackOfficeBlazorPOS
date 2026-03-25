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
    public class SalesRepositroy : ISalesRepository
    {
        private readonly BackOfficeAdminContext _context;
        private readonly IStockMovementEngine _stockMovement;

        public SalesRepositroy(BackOfficeAdminContext context, IStockMovementEngine stockMovement)
        {
            _context = context;
            _stockMovement = stockMovement;
        }
        public async Task<string> GenerateNextInvoiceNumberAsync(string location, string terminal)
        {
            // TEMP: timestamp based (replace with your real logic later)
            var invoiceNo = $"{location}{terminal}{DateTime.Now:yyyyMMddHHmmss}";
            return await Task.FromResult(invoiceNo);
        }

        public async Task UpdateStockAfterSaleAsync(PosSaleLineDto line, string location)
        {
            if (ShouldSkipComboHeader(line))
                return;

            if (line.Quantity <= 0)
                return;

            var effectiveLocation = !string.IsNullOrWhiteSpace(location) ? location : line.Location;

            if (line.IsMajor)
            {
                var stockNo = line.StockNo?.Trim();
                if (string.IsNullOrWhiteSpace(stockNo))
                    throw new InvalidOperationException($"Stock number is required for {line.PartNumber}.");

                await _stockMovement.SellMajorAsync(line.PartNumber, stockNo, effectiveLocation);
                return;
            }

            await _stockMovement.SellMinorAsync(line.PartNumber, line.Quantity, effectiveLocation);
        }

        private static bool ShouldSkipComboHeader(PosSaleLineDto line)
            => line.IsCombo &&
               line.ComboItems.Any() &&
               string.IsNullOrWhiteSpace(line.ComboGroupId);

        public async Task InsertFtt05Async(string invoiceNo, PosSaleRequestDto header, PosSaleLineDto line)
        {
            try
            {
                var grossLineTotal = line.Sell * line.Quantity;
                var discountTotal = line.DiscountAmount * line.Quantity;
                var discountPartNumber = await GetConfiguredDiscountPartNumberAsync();
                var normalizedDiscountPart = string.IsNullOrWhiteSpace(discountPartNumber)
                    ? string.Empty
                    : discountPartNumber.Trim();

                var entity = new FTT05
                {
                    DateAndTime = DateTime.Now,
                    Date = DateTime.Now.ToString("yyyyMMdd"),
                    Time = DateTime.Now.ToString("HHmmss"),

                    Location = header.Location,
                    InvoiceNumber = invoiceNo,

                    PartNumber = line.PartNumber,
                    Description = line.Description ?? "",
                    ComboId = line.ComboId,
                    IsCombo = line.IsCombo,
                    ComboGroupId = line.ComboGroupId ?? "",
                    IsComboReturnPolicyApplied = line.IsComboReturnPolicyApplied,
                    StockNo = line.StockNo ?? "",

                    Quantity = line.Quantity,

                    Cost = line.Cost,
                    Sell = line.Sell,
                    VAT = line.Vat,
                    Net = grossLineTotal,
                    Profit = line.Sell - line.Cost,

                    SalesPerson = header.SalesPerson,
                    Customer = header.Customer,
                    Terminal = header.Terminal,

                    InOut = "O",   // OUT
                    PaymentType = "", // set later if needed
                    Source = "PO"
                };

                _context.FTT05.Add(entity);

                if (discountTotal > 0)
                {
                _context.FTT05.Add(new FTT05
                {
                    DateAndTime = DateTime.Now,
                    Date = DateTime.Now.ToString("yyyyMMdd"),
                    Time = DateTime.Now.ToString("HHmmss"),
                    Location = header.Location,
                    InvoiceNumber = invoiceNo,
                    PartNumber = normalizedDiscountPart,
                    Description = $"DISCOUNT {line.PartNumber}".Trim(),
                    StockNo = "",
                    Quantity = 1,
                    Cost = 0,
                    Sell = -discountTotal,
                    VAT = 0,
                    Net = -discountTotal,
                    Profit = -discountTotal,
                    SalesPerson = header.SalesPerson,
                    Customer = header.Customer,
                    Terminal = header.Terminal,
                    InOut = "O",
                    PaymentType = "",
                    Source = "PO",
                    DiscountCode = "DI"
                });
            }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("InsertFtt05 failed: " + ex.Message, ex);
            }


        }

        private Task<string?> GetConfiguredDiscountPartNumberAsync()
            => _context.SysOptions
                .OrderByDescending(x => x.Id)
                .Select(x => x.DiscountPartNumber)
                .FirstOrDefaultAsync();
        public async Task InsertFtt11Async(string invoiceNo,PosSaleRequestDto header)
        {
            try
            {
                var p = header.Payment;
                var entity = new FTT11
                {
                    Location = header.Location,
                    InvoiceNumber = invoiceNo,

                    Amount = p.TotalTendered,

                    Cash = p.Cash,
                    Cheque = p.Cheque,
                    Credit = p.Credit,

                    Type3Description = "Master Card",
                    Type3 = p.MasterCard,

                    Type4Description = "Visa",
                    Type4 = p.Visa
                };

                _context.FTT11.Add(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("InsertFtt05 failed: " + ex.Message, ex);
            }




        }

    }
}




//using BackOfficeBlazor.Admin.Context;
//using BackOfficeBlazor.Admin.Entities;
//using BackOfficeBlazor.Admin.Repository.Interfaces;
//using BackOfficeBlazor.Shared.DTOs;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Reflection;
//using System.Threading.Tasks;

//namespace BackOfficeBlazor.Admin.Repository.Implementations
//{
//    public class SalesRepositroy : ISalesRepository
//    {
//        private readonly BackOfficeAdminContext _context;

//        public SalesRepositroy(BackOfficeAdminContext context)
//        {
//            _context = context;
//        }

//        public async Task<string> GenerateNextInvoiceNumberAsync(string location, string terminal)
//        {
//            var invoiceNo = $"{location}{terminal}{DateTime.Now:yyyyMMddHHmmss}";
//            return await Task.FromResult(invoiceNo);
//        }

//        private async Task DecrementStockLevelAsync(string partNumber, string locationCode, int quantity)
//        {
//            if (quantity <= 0)
//                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

//            var level = await _context._ProductsStockLevels
//                .FirstOrDefaultAsync(x => x.PartNumber == partNumber);

//            if (level == null)
//                throw new InvalidOperationException($"Stock level row not found for {partNumber}");

//            var prop = typeof(StockLevels).GetProperty($"L{locationCode}", BindingFlags.Public |BindingFlags.Instance);

//            if (prop == null)
//                throw new InvalidOperationException($"Invalid location code {locationCode}");

//            var current = (int)prop.GetValue(level)!;

//            if (current < quantity)
//                throw new InvalidOperationException($"Stock level underflow for {partNumber} at {locationCode}");

//            prop.SetValue(level, current - quantity);
//            level.DateUpdated = DateTime.UtcNow;
//        }

//        private async Task MarkSerialAsSoldAsync(Stock serial)
//        {
//            serial.IsAvailable = false;
//            serial.IsAvailable = false;
//            serial.DateCreated = DateTime.UtcNow;
//            _context._ProductsStock.Update(serial);       
//            await Task.CompletedTask;
//        }

//        public async Task UpdateStockAfterSaleAsync(string partNumber, string stockNo, string location, int qty)
//        {
//            if (qty <= 0)
//                throw new ArgumentException("Quantity must be greater than zero", nameof(qty));

//            if (string.IsNullOrWhiteSpace(stockNo))
//            {
//                // Non-serialized (minor) product
//                await DecrementStockLevelAsync(partNumber, location, qty);
//                await _context.SaveChangesAsync();
//                return;
//            }

//            // Serialized (major) product: one serial per unit
//            if (qty != 1)
//                throw new InvalidOperationException("Serialized stock can only be deducted one unit at a time");

//            var serial = await _context._ProductsStock
//                .FirstOrDefaultAsync(x =>
//                    x.PartNumber == partNumber &&
//                    x.StockNumber == stockNo &&
//                    x.LocationCode == location &&
//                    x.IsAvailable == true);

//            if (serial == null)
//                throw new InvalidOperationException($"Serial {stockNo} not available for {partNumber} at {location}");

//            await MarkSerialAsSoldAsync(serial);
//            await DecrementStockLevelAsync(partNumber, location, 1);

//            await _context.SaveChangesAsync();
//        }

//        public async Task InsertFtt05Async(string invoiceNo, PosSaleRequestDto header, PosSaleLineDto line)
//        {
//            try
//            {
//                var grossLineTotal = line.Sell * line.Quantity;
//                var discountTotal = line.DiscountAmount * line.Quantity;

//                var entity = new FTT05
//                {
//                    DateAndTime = DateTime.Now,
//                    Date = DateTime.Now.ToString("yyyyMMdd"),
//                    Time = DateTime.Now.ToString("HHmmss"),

//                    Location = header.Location,
//                    InvoiceNumber = invoiceNo,

//                    PartNumber = line.PartNumber,
//                    Description = line.Description ?? "",
//                    ComboId = line.ComboId,
//                    IsCombo = line.IsCombo,
//                    ComboGroupId = line.ComboGroupId ?? "",
//                    IsComboReturnPolicyApplied = line.IsComboReturnPolicyApplied,
//                    StockNo = line.StockNo ?? "",

//                    Quantity = line.Quantity,

//                    Cost = line.Cost,
//                    Sell = line.Sell,
//                    VAT = line.Vat,
//                    Net = grossLineTotal,
//                    Profit = line.Sell - line.Cost,

//                    SalesPerson = header.SalesPerson,
//                    Customer = header.Customer,
//                    Terminal = header.Terminal,

//                    InOut = "O",
//                    PaymentType = "",
//                    Source = "PO"
//                };

//                _context.FTT05.Add(entity);

//                if (discountTotal > 0)
//                {
//                    _context.FTT05.Add(new FTT05
//                    {
//                        DateAndTime = DateTime.Now,
//                        Date = DateTime.Now.ToString("yyyyMMdd"),
//                        Time = DateTime.Now.ToString("HHmmss"),
//                        Location = header.Location,
//                        InvoiceNumber = invoiceNo,
//                        PartNumber = "",
//                        Description = $"DISCOUNT {line.PartNumber}".Trim(),
//                        StockNo = "",
//                        Quantity = 1,
//                        Cost = 0,
//                        Sell = -discountTotal,
//                        VAT = 0,
//                        Net = -discountTotal,
//                        Profit = -discountTotal,
//                        SalesPerson = header.SalesPerson,
//                        Customer = header.Customer,
//                        Terminal = header.Terminal,
//                        InOut = "O",
//                        PaymentType = "",
//                        Source = "PO",
//                        DiscountCode = "DI"
//                    });
//                }

//                await _context.SaveChangesAsync();
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("InsertFtt05 failed: " + ex.Message, ex);
//            }
//        }

//        public async Task InsertFtt11Async(string invoiceNo, PosSaleRequestDto header)
//        {
//            try
//            {
//                var p = header.Payment;
//                var entity = new FTT11
//                {
//                    Location = header.Location,
//                    InvoiceNumber = invoiceNo,

//                    Amount = p.TotalTendered,

//                    Cash = p.Cash,
//                    Cheque = p.Cheque,
//                    Credit = p.Credit,

//                    Type3Description = "Master Card",
//                    Type3 = p.MasterCard,

//                    Type4Description = "Visa",
//                    Type4 = p.Visa
//                };

//                _context.FTT11.Add(entity);
//                await _context.SaveChangesAsync();
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("InsertFtt11 failed: " + ex.Message, ex);
//            }
//        }
//    }
//}

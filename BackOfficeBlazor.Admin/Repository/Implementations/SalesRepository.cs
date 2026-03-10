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

        public SalesRepositroy(BackOfficeAdminContext context)
        {
            _context = context;
        }
        public async Task<string> GenerateNextInvoiceNumberAsync(string location, string terminal)
        {
            // TEMP: timestamp based (replace with your real logic later)
            var invoiceNo = $"{location}{terminal}{DateTime.Now:yyyyMMddHHmmss}";
            return await Task.FromResult(invoiceNo);
        }
        private async Task DecrementStockLevelAsync(string partNumber,string locationCode,int quantity)
        {
            var level = await _context._ProductsStockLevels
                .FirstOrDefaultAsync(x => x.PartNumber == partNumber);

            if (level == null)
                throw new Exception($"Stock level row not found for {partNumber}");

            var prop = typeof(StockLevels)
                .GetProperty($"L{locationCode}");

            if (prop == null)
                throw new Exception($"Invalid location code {locationCode}");

            var current = (int)prop.GetValue(level)!;

            if (current < quantity)
                throw new Exception(
                    $"Stock level underflow for {partNumber} at {locationCode}");

            prop.SetValue(level, current - quantity);

            level.DateUpdated = DateTime.UtcNow;
        }

        public async Task UpdateStockAfterSaleAsync(
            string partNumber,
            string stockNo,
            string location,
            int qty)
        {
            // ================= MINOR PRODUCT =================
            if (string.IsNullOrEmpty(stockNo))
            {
                var stock = await _context._ProductsStock
                    .FirstOrDefaultAsync(x =>
                        x.PartNumber == partNumber &&
                        x.LocationCode == location);

                if (stock == null)
                    throw new Exception($"Stock not found for {partNumber} at {location}");

                if (stock.Quantity < qty)
                    throw new Exception($"Insufficient stock for {partNumber} at {location}");

                // 1️⃣ Decrement detailed stock
                //stock.Quantity -= qty;

                // 2️⃣ Decrement stock levels summary
                await DecrementStockLevelAsync(partNumber, location, qty);

                await _context.SaveChangesAsync();
            }
            // ================= MAJOR PRODUCT (SERIAL) =================
            else
            {
                var serial = await _context._ProductsStock
                    .FirstOrDefaultAsync(x =>
                        x.PartNumber == partNumber &&
                        x.StockNumber == stockNo &&
                        x.LocationCode == location &&
                        x.IsAvailable == true);

                if (serial == null)
                    throw new Exception($"Serial {stockNo} not available for {partNumber} at {location}");

                // 1️⃣ Mark serial unavailable
                serial.IsAvailable = false;
                // serial.IsSold = true; // if you add later

                // 2️⃣ Decrement stock levels summary by 1
                await DecrementStockLevelAsync(partNumber, location, 1);

                await _context.SaveChangesAsync();
            }
        }

        public async Task InsertFtt05Async(string invoiceNo, PosSaleRequestDto header,PosSaleLineDto line)
        {
            try
            {
                var entity = new FTT05
                {
                    DateAndTime = DateTime.Now,
                    Date = DateTime.Now.ToString("yyyyMMdd"),
                    Time = DateTime.Now.ToString("HHmmss"),

                    Location = header.Location,
                    InvoiceNumber = invoiceNo,

                    PartNumber = line.PartNumber,
                    StockNo = line.StockNo ?? "",

                    Quantity = line.Quantity,

                    Cost = line.Cost,
                    Sell = line.Sell,
                    VAT = line.Vat,
                    Net = line.Net,
                    Profit = line.Sell - line.Cost,

                    SalesPerson = header.SalesPerson,
                    Customer = header.Customer,
                    Terminal = header.Terminal,

                    InOut = "O",   // OUT
                    PaymentType = "", // set later if needed
                    Source = "PO"
                };

                _context.FTT05.Add(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("InsertFtt05 failed: " + ex.Message, ex);
            }

     
        }
        public async Task InsertFtt11Async(string invoiceNo,
                                           PosSaleRequestDto header)
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

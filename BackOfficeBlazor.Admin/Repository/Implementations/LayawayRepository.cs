using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class LayawayRepository : ILayawayRepository
    {
        private readonly BackOfficeAdminContext _context;

        public LayawayRepository(BackOfficeAdminContext context)
        {
            _context = context;
        }

        public async Task<int> CreateAsync(LayawayCreateDto request)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var nextRef = await _context.Layaways.MaxAsync(x => (int?)x.ReferenceNo) ?? 0;
                var layawayNo = nextRef + 1;

                foreach (var line in request.Lines)
                {
                    ValidateLine(line);
                    await DecrementStockAsync(line, request.Location);

                    var entity = new Layaway
                    {
                        ReferenceNo = layawayNo,
                        CustomerAccNo = request.CustomerAccNo,
                        Location = request.Location,
                        SalesPerson = request.SalesPerson,
                        PartNumber = line.PartNumber,
                        StockNo = line.StockNo ?? "",
                        Quantity = line.Quantity,
                        Sell = line.Sell,
                        Reserved = true,
                        LayawayType = 0,
                        Date = DateTime.Now
                    };

                    _context.Layaways.Add(entity);
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return layawayNo;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<List<LayawaySummaryDto>> GetActiveAsync(string? customerAccNo)
        {
            var query = _context.Layaways.AsNoTracking()
                .Where(x => x.Reserved == true);

            if (!string.IsNullOrWhiteSpace(customerAccNo))
                query = query.Where(x => x.CustomerAccNo == customerAccNo);

            return await query
                .GroupBy(x => new { x.ReferenceNo, x.CustomerAccNo, x.Date })
                .Select(g => new LayawaySummaryDto
                {
                    LayawayNo = g.Key.ReferenceNo ?? 0,
                    CustomerAccNo = g.Key.CustomerAccNo ?? "",
                    DateCreated = g.Key.Date ?? DateTime.MinValue,
                    TotalNet = g.Sum(x => (x.Sell ?? 0) * (x.Quantity ?? 0)),
                    ItemsCount = g.Sum(x => x.Quantity ?? 0)
                })
                .OrderByDescending(x => x.DateCreated)
                .ToListAsync();
        }

        public async Task<LayawayDetailDto?> GetAsync(int layawayNo)
        {
            var lines = await _context.Layaways.AsNoTracking()
                .Where(x => x.ReferenceNo == layawayNo)
                .OrderBy(x => x.PartNumber)
                .ToListAsync();

            if (lines.Count == 0)
                return null;

            var header = lines[0];

            return new LayawayDetailDto
            {
                LayawayNo = header.ReferenceNo ?? 0,
                CustomerAccNo = header.CustomerAccNo ?? "",
                Location = header.Location ?? "",
                Terminal = "",
                SalesPerson = header.SalesPerson ?? "",
                DateCreated = header.Date ?? DateTime.MinValue,
                Status = header.Reserved == true ? "A" : "X",
                Lines = lines.Select(x => new LayawayLineDto
                {
                    PartNumber = x.PartNumber ?? "",
                    StockNo = x.StockNo ?? "",
                    Quantity = x.Quantity ?? 0,
                    Cost = x.Sell ?? 0,
                    Sell = x.Sell ?? 0,
                    Net = (x.Sell ?? 0) * (x.Quantity ?? 0),
                    Vat = 0,
                    IsMajor = !string.IsNullOrWhiteSpace(x.StockNo)
                }).ToList()
            };
        }

        public async Task AddLineAsync(int layawayNo, LayawayLineDto line)
        {
            ValidateLine(line);

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var header = await _context.Layaways
                    .FirstOrDefaultAsync(x => x.ReferenceNo == layawayNo && x.Reserved == true);

                if (header == null)
                    throw new Exception("Layaway not found or not active");

                var existing = await _context.Layaways
                    .FirstOrDefaultAsync(x =>
                        x.ReferenceNo == layawayNo &&
                        x.PartNumber == line.PartNumber &&
                        x.StockNo == (line.StockNo ?? "") &&
                        x.Reserved == true);

                if (existing != null)
                {
                    if (!string.IsNullOrWhiteSpace(existing.StockNo))
                        throw new Exception("Serialized item already exists in layaway");

                    var newQty = (existing.Quantity ?? 0) + line.Quantity;
                    await UpdateLineQtyInternal(existing, newQty, header.Location ?? "");
                }
                else
                {
                    await DecrementStockAsync(line, header.Location ?? "");

                    var entity = new Layaway
                    {
                        ReferenceNo = layawayNo,
                        CustomerAccNo = header.CustomerAccNo,
                        Location = header.Location,
                        SalesPerson = header.SalesPerson,
                        PartNumber = line.PartNumber,
                        StockNo = line.StockNo ?? "",
                        Quantity = line.Quantity,
                        Sell = line.Sell,
                        Reserved = true,
                        LayawayType = 0,
                        Date = header.Date
                    };

                    _context.Layaways.Add(entity);
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateLineQtyAsync(int layawayNo, LayawayLineUpdateDto update)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var line = await _context.Layaways
                    .FirstOrDefaultAsync(x =>
                        x.ReferenceNo == layawayNo &&
                        x.PartNumber == update.PartNumber &&
                        x.StockNo == (update.StockNo ?? "") &&
                        x.Reserved == true);

                if (line == null)
                    throw new Exception("Layaway line not found");

                await UpdateLineQtyInternal(line, update.NewQty, line.Location ?? "");

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task ReverseAsync(int layawayNo)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var lines = await _context.Layaways
                    .Where(x => x.ReferenceNo == layawayNo && x.Reserved == true)
                    .ToListAsync();

                if (lines.Count == 0)
                    throw new Exception("Layaway not found or already processed");

                foreach (var line in lines)
                {
                    await IncrementStockAsync(line);
                    line.Reserved = false;
                    line.LayawayType = 2;
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<List<LayawayLineDto>> GetLinesAsync(int layawayNo)
        {
            return await _context.Layaways.AsNoTracking()
                .Where(x => x.ReferenceNo == layawayNo)
                .Select(x => new LayawayLineDto
                {
                    PartNumber = x.PartNumber ?? "",
                    StockNo = x.StockNo ?? "",
                    Quantity = x.Quantity ?? 0,
                    Cost = x.Sell ?? 0,
                    Sell = x.Sell ?? 0,
                    Net = (x.Sell ?? 0) * (x.Quantity ?? 0),
                    Vat = 0,
                    IsMajor = !string.IsNullOrWhiteSpace(x.StockNo)
                })
                .ToListAsync();
        }

        public async Task MarkStatusAsync(int layawayNo, int layawayType, bool reserved)
        {
            var lines = await _context.Layaways
                .Where(x => x.ReferenceNo == layawayNo && x.Reserved == true)
                .ToListAsync();

            foreach (var line in lines)
            {
                line.Reserved = reserved;
                line.LayawayType = layawayType;
            }

            await _context.SaveChangesAsync();
        }

        private void ValidateLine(LayawayLineDto line)
        {
            if (string.IsNullOrWhiteSpace(line.PartNumber))
                throw new Exception("PartNumber is required");

            if (line.Quantity <= 0)
                throw new Exception("Quantity must be greater than zero");

            if (!string.IsNullOrWhiteSpace(line.StockNo) && line.Quantity != 1)
                throw new Exception("Serialized item quantity must be 1");
        }

        private async Task UpdateLineQtyInternal(Layaway line, int newQty, string location)
        {
            if (!string.IsNullOrWhiteSpace(line.StockNo))
            {
                if (newQty != 0 && newQty != 1)
                    throw new Exception("Serialized item quantity must be 0 or 1");
            }

            var currentQty = line.Quantity ?? 0;
            var delta = newQty - currentQty;

            if (delta > 0)
            {
                var addLine = new LayawayLineDto
                {
                    PartNumber = line.PartNumber ?? "",
                    StockNo = line.StockNo ?? "",
                    Quantity = delta,
                    IsMajor = !string.IsNullOrWhiteSpace(line.StockNo)
                };
                await DecrementStockAsync(addLine, location);
            }
            else if (delta < 0)
            {
                await IncrementStockAsync(line, -delta);
            }

            if (newQty == 0)
            {
                _context.Layaways.Remove(line);
            }
            else
            {
                line.Quantity = newQty;
            }
        }

        private async Task DecrementStockAsync(LayawayLineDto line, string location)
        {
            if (string.IsNullOrWhiteSpace(line.StockNo))
            {
                var stock = await _context._ProductsStock
                    .FirstOrDefaultAsync(x =>
                        x.PartNumber == line.PartNumber &&
                        x.LocationCode == location);

                if (stock == null)
                    throw new Exception($"Stock not found for {line.PartNumber} at {location}");

                if (stock.Quantity < line.Quantity)
                    throw new Exception($"Insufficient stock for {line.PartNumber} at {location}");

                stock.Quantity -= line.Quantity;

                await DecrementStockLevelAsync(line.PartNumber, location, line.Quantity);
            }
            else
            {
                var serial = await _context._ProductsStock
                    .FirstOrDefaultAsync(x =>
                        x.PartNumber == line.PartNumber &&
                        x.StockNumber == line.StockNo &&
                        x.LocationCode == location &&
                        x.IsAvailable == true);

                if (serial == null)
                    throw new Exception($"Serial {line.StockNo} not available for {line.PartNumber} at {location}");

                serial.IsAvailable = false;

                await DecrementStockLevelAsync(line.PartNumber, location, 1);
            }
        }

        private async Task IncrementStockAsync(Layaway line, int qtyOverride = 0)
        {
            var qty = qtyOverride > 0 ? qtyOverride : (line.Quantity ?? 0);

            if (string.IsNullOrWhiteSpace(line.StockNo))
            {
                var stock = await _context._ProductsStock
                    .FirstOrDefaultAsync(x =>
                        x.PartNumber == line.PartNumber &&
                        x.LocationCode == line.Location);

                if (stock != null)
                    stock.Quantity += qty;

                await IncrementStockLevelAsync(line.PartNumber ?? "", line.Location ?? "", qty);
            }
            else
            {
                var serial = await _context._ProductsStock
                    .FirstOrDefaultAsync(x =>
                        x.PartNumber == line.PartNumber &&
                        x.StockNumber == line.StockNo &&
                        x.LocationCode == line.Location);

                if (serial != null)
                    serial.IsAvailable = true;

                await IncrementStockLevelAsync(line.PartNumber ?? "", line.Location ?? "", 1);
            }
        }

        private async Task DecrementStockLevelAsync(string partNumber, string locationCode, int quantity)
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
                throw new Exception($"Stock level underflow for {partNumber} at {locationCode}");

            prop.SetValue(level, current - quantity);
            level.DateUpdated = DateTime.UtcNow;
        }

        private async Task IncrementStockLevelAsync(string partNumber, string locationCode, int quantity)
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
            prop.SetValue(level, current + quantity);
            level.DateUpdated = DateTime.UtcNow;
        }
    }
}

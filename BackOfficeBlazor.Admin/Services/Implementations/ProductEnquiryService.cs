using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    public class ProductEnquiryService : IProductEnquiryService
    {
        private readonly BackOfficeAdminContext _context;

        public ProductEnquiryService(BackOfficeAdminContext context)
        {
            _context = context;
        }

        public async Task<ProductEnquiryHeaderDto?> GetProductHeaderAsync(string partNumber)
        {
            if (string.IsNullOrWhiteSpace(partNumber))
                return null;

            partNumber = partNumber.Trim().ToUpperInvariant();
            var product = await _context.ProductItems
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PartNumber.ToUpper() == partNumber);

            if (product == null)
                return null;

            var stockQuery = _context._ProductsStock
                .AsNoTracking()
                .Where(x => x.PartNumber.ToUpper() == partNumber);

            var totalStock = await stockQuery.SumAsync(x => (int?)x.Quantity) ?? 0;
            var latestCost = await stockQuery
                .OrderByDescending(x => x.DateCreated)
                .Select(x => (decimal?)x.Cost)
                .FirstOrDefaultAsync() ?? 0m;
            var averageCost = await stockQuery.AverageAsync(x => (decimal?)x.Cost) ?? 0m;
            var allocatedStock = await stockQuery
                .Where(x => !x.IsAvailable && !x.IsCollected)
                .SumAsync(x => (int?)x.Quantity) ?? 0;

            var salesQuery = _context.FTT05
                .AsNoTracking()
                .Where(x => x.PartNumber == partNumber && !string.IsNullOrWhiteSpace(x.InOut) && x.InOut.ToUpper().StartsWith("S"));

            var salesList = await salesQuery.ToListAsync();
            var totalQtySold = salesList.Sum(x => x.Quantity);
            var totalSalesValue = salesList.Sum(x => x.Sell);
            var totalRevenue = salesList.Sum(x => x.Sell * x.Quantity);
            var totalVat = salesList.Sum(x => x.VAT);
            var lastSoldDate = salesList
                .Where(x => x.DateAndTime != null)
                .OrderByDescending(x => x.DateAndTime)
                .Select(x => x.DateAndTime)
                .FirstOrDefault();

            var faultyStock = await _context.ReturnFaultyItems
                .AsNoTracking()
                .Where(x => x.ProductId == partNumber)
                .SumAsync(x => (int?)x.Qty) ?? 0;

            var productLevel = await _context.ProductLevels
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PartNumber == partNumber);

            var stockSnapshot = await _context._ProductsStockLevels
                .AsNoTracking()
                .Where(x => x.PartNumber == partNumber)
                .OrderByDescending(x => x.DateCreated)
                .FirstOrDefaultAsync();

            int minStock = 0;
            int maxStock = 0;
            if (productLevel != null)
            {
                minStock = productLevel.Min01 + productLevel.Min02 + productLevel.Min03;
                maxStock = productLevel.Max01 + productLevel.Max02 + productLevel.Max03;
            }

            var categoryHierarchy = string.Join(" > ",
                new[] { product.CatA, product.CatB, product.CatC }
                    .Where(x => !string.IsNullOrWhiteSpace(x)));

            decimal marginPercent = 0;
            if (totalRevenue > 0)
            {
                marginPercent = ((totalRevenue - (averageCost * totalQtySold)) / totalRevenue) * 100m;
            }

            return new ProductEnquiryHeaderDto
            {
                PartNumber = product.PartNumber,
                MfrPartNumber = product.MfrPartNumber,
                Barcode = product.Barcode,
                Make = product.Make,
                ImageUrl = product.ImageMain,
                CategoryHierarchy = categoryHierarchy,
                SearchKeywords = string.Join(" ", new[] { product.Search1, product.Search2 }.Where(x => !string.IsNullOrWhiteSpace(x))),
                Size = product.Size,
                Color = product.Color,
                BrandCode = product.MakeCode,
                Supplier1 = product.Supplier1Code,
                Supplier2 = product.Supplier2Code,
                TotalStock = totalStock,
                LatestCost = latestCost,
                AverageCost = averageCost,
                SuggestedRrp = product.SuggestedRRP,
                LastSoldDate = lastSoldDate,
                TotalQtySold = totalQtySold,
                TotalSalesValue = totalSalesValue,
                TotalRevenue = totalRevenue,
                TotalVat = totalVat,
                FaultyStockQty = faultyStock,
                BackOrdersQty = 0,
                MinStock = minStock,
                MaxStock = maxStock,
                AllocatedStock = allocatedStock,
                LastStockCheck = stockSnapshot?.DateCreated,
                MarginPercent = marginPercent
            };
        }

        public async Task<ProductEnquiryHistoryDto?> GetProductHistoryAsync(string partNumber)
        {
            partNumber = NormalizePartNumber(partNumber);

            if (string.IsNullOrWhiteSpace(partNumber))
                return null;

            var now = DateTime.UtcNow;
            var baseMonth = new DateTime(now.Year, now.Month, 1);
            var months = Enumerable.Range(0, 12)
                .Select(offset => baseMonth.AddMonths(-offset))
                .OrderBy(d => d)
                .ToList();

            var historyMap = months.ToDictionary(
                key => key,
                key => new ProductEnquiryHistoryRowDto
                {
                    Month = key.ToString("MMM yyyy")
                });

            var sales = await _context.FTT05
                .AsNoTracking()
                .Where(x => x.PartNumber == partNumber &&
                            !string.IsNullOrWhiteSpace(x.InOut) &&
                            x.InOut.ToUpper().StartsWith("S"))
                .ToListAsync();

            var receipts = await _context._ProductsStock
                .AsNoTracking()
                .Where(x => x.PartNumber == partNumber)
                .ToListAsync();

            foreach (var sale in sales)
            {
                var date = GetFtt05Date(sale);
                if (date == DateTime.MinValue)
                    continue;

                var key = new DateTime(date.Year, date.Month, 1);
                if (!historyMap.TryGetValue(key, out var row))
                    continue;

                row.UnitsSold += sale.Quantity;
                row.Turnover += sale.Sell * sale.Quantity;
                row.Profit += sale.Profit;
            }

            foreach (var receipt in receipts)
            {
                var key = new DateTime(receipt.DateCreated.Year, receipt.DateCreated.Month, 1);
                if (!historyMap.TryGetValue(key, out var row))
                    continue;

                row.UnitsIn += receipt.Quantity;
            }

            var totalSold = historyMap.Values.Sum(x => x.UnitsSold);
            var suggested = (int)Math.Ceiling(totalSold / 12m * 1.2m);

            return new ProductEnquiryHistoryDto
            {
                SuggestedStockLevel = suggested,
                MonthlySummaries = historyMap.OrderBy(kvp => kvp.Key)
                    .Select(kvp => kvp.Value)
                    .ToList()
            };
        }

        public async Task<List<ProductEnquiryLocationDto>> GetLocationDistributionAsync(string partNumber)
        {
            partNumber = NormalizePartNumber(partNumber);

            if (string.IsNullOrWhiteSpace(partNumber))
                return new();

            var startPeriod = DateTime.UtcNow.AddMonths(-12);

            var locations = await _context._Locations
                .AsNoTracking()
                .ToDictionaryAsync(l => l.Code, StringComparer.OrdinalIgnoreCase);

            var stockRows = await _context._ProductsStock
                .AsNoTracking()
                .Where(x => x.PartNumber == partNumber)
                .ToListAsync();

            var returnLines = await _context.ReturnItemTracking
                .AsNoTracking()
                .Where(x => x.ProductId == partNumber)
                .GroupBy(x => x.StoreId)
                .Select(g => new { Location = g.Key, Qty = g.Sum(x => x.Qty) })
                .ToListAsync();

            var faultyLines = await _context.ReturnFaultyItems
                .AsNoTracking()
                .Where(x => x.ProductId == partNumber)
                .GroupBy(x => x.StoreId)
                .Select(g => new { Location = g.Key, Qty = g.Sum(x => x.Qty) })
                .ToListAsync();

            var sales = await _context.FTT05
                .AsNoTracking()
                .Where(x => x.PartNumber == partNumber)
                .ToListAsync();

            var locationSet = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var locationCode in locations.Keys)
                locationSet.Add(locationCode);

            foreach (var stock in stockRows.Select(x => x.LocationCode).Where(x => !string.IsNullOrWhiteSpace(x)))
                locationSet.Add(stock);

            foreach (var sale in sales.Select(x => x.Location).Where(x => !string.IsNullOrWhiteSpace(x)))
                locationSet.Add(sale!);

            var returnedByLocation = returnLines
                .Concat(faultyLines)
                .GroupBy(x => x.Location, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key ?? string.Empty, g => g.Sum(x => x.Qty), StringComparer.OrdinalIgnoreCase);

            var salesByLocation = sales
                .Where(x => GetFtt05Date(x) >= startPeriod)
                .GroupBy(x => x.Location ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity), StringComparer.OrdinalIgnoreCase);

            var productLevel = await _context.ProductLevels
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PartNumber == partNumber);

            var results = new List<ProductEnquiryLocationDto>();

            foreach (var code in locationSet)
            {
                var entries = stockRows.Where(x => string.Equals(x.LocationCode, code, StringComparison.OrdinalIgnoreCase)).ToList();
                if (!entries.Any() && !salesByLocation.ContainsKey(code) && !returnedByLocation.ContainsKey(code))
                    continue;

                var (min, max) = GetMinMaxForLocation(productLevel, code);
                var reserved = entries.Where(x => !x.IsAvailable && !x.IsCollected).Sum(x => x.Quantity);
                var backOrders = entries.Where(x => !x.IsAvailable && x.IsCollected).Sum(x => x.Quantity);
                returnedByLocation.TryGetValue(code, out var returnedQty);
                salesByLocation.TryGetValue(code, out var soldQty);

                results.Add(new ProductEnquiryLocationDto
                {
                    LocationCode = code,
                    LocationName = locations.TryGetValue(code, out var location) ? location.Name : code,
                    InStockQty = entries.Sum(x => x.Quantity),
                    StockValue = entries.Sum(x => x.Quantity * x.Cost),
                    ReservedQty = reserved,
                    BackOrdersQty = backOrders,
                    ReturnedQty = returnedQty,
                    SalesLast12Months = soldQty,
                    MinStock = min,
                    MaxStock = max
                });
            }

            return results;
        }

        public async Task<List<ProductEnquiryTransferDto>> GetTransfersAsync(string partNumber)
        {
            partNumber = NormalizePartNumber(partNumber);

            if (string.IsNullOrWhiteSpace(partNumber))
                return new();

            var transfers = await _context.ProductStockMovement
                .AsNoTracking()
                .Where(x => x.PartNo == partNumber)
                .OrderByDescending(x => x.DateAndTime)
                .Take(200)
                .ToListAsync();

            return transfers.Select(x => new ProductEnquiryTransferDto
            {
                DispatchId = x.Id,
                Date = x.DateAndTime,
                FromLocation = x.FromLocation,
                ToLocation = x.ToLocation,
                Qty = x.StockQty,
                SalesCode = x.SalesCode,
                SerialNumber = string.Empty
            }).ToList();
        }

        public async Task<List<ProductEnquiryPurchaseOrderDto>> GetPurchaseOrdersAsync(string partNumber)
        {
            partNumber = NormalizePartNumber(partNumber);

            if (string.IsNullOrWhiteSpace(partNumber))
                return new();

            var orders = await _context._ProductsStock
                .AsNoTracking()
                .Where(x => x.PartNumber == partNumber)
                .GroupBy(x => x.PurchaseOrderNo)
                .Select(g => new
                {
                    OrderNo = g.Key ?? string.Empty,
                    Qty = g.Sum(x => x.Quantity),
                    Outstanding = g.Where(x => !x.IsAvailable).Sum(x => x.Quantity),
                    DateCreated = g.Min(x => x.DateCreated),
                    Location = g.Select(x => x.LocationCode).FirstOrDefault(),
                    Customer = g.Select(x => x.CustomerAccNo).FirstOrDefault(),
                    Cost = g.Sum(x => x.Quantity * x.Cost),
                    Supplier = g.Select(x => x.SupplierCode).FirstOrDefault(),
                    StatusCode = g.Select(x => x.StockItemStatus).FirstOrDefault()
                })
                .OrderByDescending(x => x.DateCreated)
                .ToListAsync();

            return orders.Select(x => new ProductEnquiryPurchaseOrderDto
            {
                OrderNo = x.OrderNo,
                Status = x.StatusCode.HasValue ? $"Status {x.StatusCode}" : "Received",
                Qty = x.Qty,
                OutstandingQty = x.Outstanding,
                Date = x.DateCreated,
                Location = x.Location ?? string.Empty,
                CustomerAccount = x.Customer ?? string.Empty,
                Cost = x.Cost,
                Supplier = x.Supplier ?? string.Empty
            }).ToList();
        }

        public async Task<List<ProductEnquiryTransactionDto>> GetTransactionsAsync(string partNumber)
        {
            partNumber = NormalizePartNumber(partNumber);

            if (string.IsNullOrWhiteSpace(partNumber))
                return new();

            var receipts = await _context._ProductsStock
                .AsNoTracking()
                .Where(x => x.PartNumber == partNumber)
                .ToListAsync();

            var sales = await _context.FTT05
                .AsNoTracking()
                .Where(x => x.PartNumber == partNumber &&
                            !string.IsNullOrWhiteSpace(x.InOut) &&
                            x.InOut.ToUpper().StartsWith("S"))
                .ToListAsync();

            var ledger = new List<(DateTime Date, string Description, int Delta, string Location, string SalesPerson)>();

            foreach (var receipt in receipts)
            {
                ledger.Add((
                    receipt.DateCreated,
                    $"Stock receive PO {receipt.PurchaseOrderNo}",
                    receipt.Quantity,
                    receipt.LocationCode,
                    receipt.StaffCode));
            }

            foreach (var sale in sales)
            {
                var date = GetFtt05Date(sale);
                if (date == DateTime.MinValue)
                    continue;

                ledger.Add((
                    date,
                    $"Sale {sale.InvoiceNumber}",
                    -sale.Quantity,
                    sale.Location,
                    sale.SalesPerson));
            }

            var ordered = ledger.OrderBy(x => x.Date).ToList();
            var running = 0;
            var result = new List<ProductEnquiryTransactionDto>();

            foreach (var entry in ordered)
            {
                running += entry.Delta;

                result.Add(new ProductEnquiryTransactionDto
                {
                    Date = entry.Date,
                    Description = entry.Description,
                    QtyIn = entry.Delta > 0 ? entry.Delta : 0,
                    QtyOut = entry.Delta < 0 ? -entry.Delta : 0,
                    Location = entry.Location,
                    StockBalanceAfter = running,
                    SalesPerson = entry.SalesPerson
                });
            }

            return result.OrderByDescending(x => x.Date).ToList();
        }

        public async Task<List<ProductEnquirySaleDto>> GetSalesAsync(string partNumber)
        {
            partNumber = NormalizePartNumber(partNumber);

            if (string.IsNullOrWhiteSpace(partNumber))
                return new();

            var sales = await _context.FTT05
                .AsNoTracking()
                .Where(x => x.PartNumber == partNumber &&
                            !string.IsNullOrWhiteSpace(x.InOut) &&
                            x.InOut.ToUpper().StartsWith("S"))
                .ToListAsync();

            var ordered = sales
                .OrderByDescending(GetFtt05Date)
                .ToList();

            return ordered.Select(x => new ProductEnquirySaleDto
            {
                Customer = x.Customer,
                Invoice = x.InvoiceNumber,
                Qty = x.Quantity,
                Price = x.Sell,
                Cost = x.Cost,
                Serial = x.StockNo,
                Balance = x.Net
            }).ToList();
        }

        public async Task<List<ProductEnquiryStockCheckDto>> GetStockChecksAsync(string partNumber)
        {
            partNumber = NormalizePartNumber(partNumber);

            if (string.IsNullOrWhiteSpace(partNumber))
                return new();

            var levels = await _context._ProductsStockLevels
                .AsNoTracking()
                .Where(x => x.PartNumber == partNumber)
                .OrderByDescending(x => x.DateCreated)
                .Take(5)
                .ToListAsync();

            return levels.Select(x => new ProductEnquiryStockCheckDto
            {
                StockCheckDate = x.DateUpdated ?? x.DateCreated,
                SystemQty = SumStockLevels(x),
                PhysicalQty = SumStockLevels(x),
                Difference = 0,
                Location = "Global"
            }).ToList();
        }

        public async Task<List<ProductEnquiryLayawayDto>> GetLayawaysAsync(string partNumber)
        {
            partNumber = NormalizePartNumber(partNumber);

            if (string.IsNullOrWhiteSpace(partNumber))
                return new();

            var layaways = await _context.Layaways
                .AsNoTracking()
                .Where(x => x.PartNumber == partNumber)
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            return layaways.Select(x => new ProductEnquiryLayawayDto
            {
                LayawayRef = x.ReferenceNo?.ToString() ?? string.Empty,
                Customer = x.CustomerAccNo ?? string.Empty,
                Qty = x.Quantity ?? 0,
                Price = x.Sell ?? 0m,
                Status = x.Reserved == true ? "Reserved" : "Open",
                Date = x.Date
            }).ToList();
        }

        public async Task<List<ProductEnquiryLogDto>> GetLogsAsync(string partNumber)
        {
            partNumber = NormalizePartNumber(partNumber);

            if (string.IsNullOrWhiteSpace(partNumber))
                return new();

            var movements = await _context.ProductStockMovement
                .AsNoTracking()
                .Where(x => x.PartNo == partNumber)
                .OrderByDescending(x => x.DateAndTime)
                .Take(100)
                .ToListAsync();

            return movements.Select(x => new ProductEnquiryLogDto
            {
                User = x.SalesCode,
                Action = "Stock movement",
                Date = x.DateAndTime,
                Description = x.Notes
            }).ToList();
        }

        public async Task<List<ProductEnquiryInternalOrderDto>> GetInternalOrdersAsync(string partNumber)
        {
            partNumber = NormalizePartNumber(partNumber);

            if (string.IsNullOrWhiteSpace(partNumber))
                return new();

            var movements = await _context.ProductStockMovement
                .AsNoTracking()
                .Where(x => x.PartNo == partNumber)
                .OrderByDescending(x => x.DateAndTime)
                .Take(100)
                .ToListAsync();

            return movements.Select(x => new ProductEnquiryInternalOrderDto
            {
                InternalOrderNo = x.Id.ToString(),
                FromLocation = x.FromLocation,
                ToLocation = x.ToLocation,
                Qty = x.StockQty,
                Status = "Completed",
                Date = x.DateAndTime
            }).ToList();
        }

        private static string NormalizePartNumber(string partNumber)
            => string.IsNullOrWhiteSpace(partNumber)
                ? string.Empty
                : partNumber.Trim().ToUpperInvariant();

        private static DateTime GetFtt05Date(FTT05 record)
        {
            if (record.DateAndTime.HasValue)
                return record.DateAndTime.Value;

            if (!string.IsNullOrWhiteSpace(record.Date))
            {
                if (!string.IsNullOrWhiteSpace(record.Time))
                {
                    if (DateTime.TryParseExact(record.Date + record.Time, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var combined))
                    {
                        return combined;
                    }
                }

                if (DateTime.TryParseExact(record.Date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                {
                    return parsed;
                }

                if (DateTime.TryParse(record.Date, out parsed))
                {
                    return parsed;
                }
            }

            return DateTime.MinValue;
        }

        private static (int Min, int Max) GetMinMaxForLocation(ProductLevel? level, string locationCode)
        {
            if (level == null || string.IsNullOrWhiteSpace(locationCode) || !int.TryParse(locationCode, out var index))
            {
                return (0, 0);
            }

            var minProp = typeof(ProductLevel).GetProperty($"Min{index:D2}");
            var maxProp = typeof(ProductLevel).GetProperty($"Max{index:D2}");

            var min = minProp?.GetValue(level) as int? ?? 0;
            var max = maxProp?.GetValue(level) as int? ?? 0;

            return (min, max);
        }

        private static int SumStockLevels(StockLevels levels)
        {
            if (levels == null)
                return 0;

            var total = 0;
            for (var i = 1; i <= 30; i++)
            {
                var prop = typeof(StockLevels).GetProperty($"L{i:00}");
                if (prop == null)
                    continue;

                total += (int)(prop.GetValue(levels) ?? 0);
            }

            return total;
        }
    }
}

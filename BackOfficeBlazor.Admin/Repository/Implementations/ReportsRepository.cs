using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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

        public async Task<List<StockPositionLineDto>> GetStockPositionAsync(StockPositionReportRequestDto request)
        {
            var locations = await _context._Locations
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => new
                {
                    x.Code,
                    x.Name
                })
                .ToListAsync();

            var slots = locations
                .Select(x => new
                {
                    Code = NormalizeLocationCode(x.Code),
                    x.Name
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Code) && TryParseSlot(x.Code, out var slot) && slot >= 1 && slot <= 3)
                .OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (slots.Count == 0)
                return new List<StockPositionLineDto>();

            var fromCode = NormalizeLocationCode(request.FromLocation);
            var toCode = NormalizeLocationCode(request.ToLocation);

            if (string.IsNullOrWhiteSpace(fromCode))
                fromCode = slots.First().Code;
            if (string.IsNullOrWhiteSpace(toCode))
                toCode = slots.Last().Code;

            if (string.CompareOrdinal(fromCode, toCode) > 0)
            {
                (fromCode, toCode) = (toCode, fromCode);
            }

            var selectedSlots = slots
                .Where(x => string.CompareOrdinal(x.Code, fromCode) >= 0 &&
                            string.CompareOrdinal(x.Code, toCode) <= 0)
                .OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (selectedSlots.Count == 0)
                return new List<StockPositionLineDto>();

            var productsQuery = _context.ProductItems.AsNoTracking().AsQueryable();

            if (request.CurrentOnly)
                productsQuery = productsQuery.Where(x => x.Current == true);

            switch ((request.PrintMode ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "major":
                    productsQuery = productsQuery.Where(x => x.Major);
                    break;
                case "minor":
                    productsQuery = productsQuery.Where(x => !x.Major);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(request.CategoryA))
            {
                var category = request.CategoryA.Trim();
                productsQuery = productsQuery.Where(x => x.CatACode == category || x.CatA == category);
            }

            if (!string.IsNullOrWhiteSpace(request.CategoryB))
            {
                var category = request.CategoryB.Trim();
                productsQuery = productsQuery.Where(x => x.CatBCode == category || x.CatB == category);
            }

            if (!string.IsNullOrWhiteSpace(request.CategoryC))
            {
                var category = request.CategoryC.Trim();
                productsQuery = productsQuery.Where(x => x.CatCCode == category || x.CatC == category);
            }

            if (!string.IsNullOrWhiteSpace(request.Supplier))
            {
                var supplier = request.Supplier.Trim();
                productsQuery = productsQuery.Where(x =>
                    x.Supplier1Code == supplier ||
                    x.Supplier2Code == supplier);
            }

            var products = await productsQuery
                .OrderBy(x => x.PartNumber)
                .ToListAsync();

            var yearBounds = NormalizeYearBounds(request.FromYear, request.ToYear);
            if (yearBounds is not null)
            {
                var (fromYear, toYear) = yearBounds.Value;
                products = products
                    .Where(x =>
                        int.TryParse(x.Year, NumberStyles.Integer, CultureInfo.InvariantCulture, out var year) &&
                        year >= fromYear &&
                        year <= toYear)
                    .ToList();
            }

            if (products.Count == 0)
                return new List<StockPositionLineDto>();

            var partNumbers = products
                .Select(x => x.PartNumber)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var stockRows = await _context._ProductsStockLevels
                .AsNoTracking()
                .Where(x => partNumbers.Contains(x.PartNumber))
                .ToListAsync();

            var levelRows = await _context.ProductLevels
                .AsNoTracking()
                .Where(x => partNumbers.Contains(x.PartNumber))
                .ToListAsync();

            var stockByPart = stockRows
                .GroupBy(x => x.PartNumber, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.DateUpdated ?? x.DateCreated).First(),
                    StringComparer.OrdinalIgnoreCase);

            var levelByPart = levelRows
                .GroupBy(x => x.PartNumber, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First(),
                    StringComparer.OrdinalIgnoreCase);

            var results = new List<StockPositionLineDto>();

            foreach (var product in products)
            {
                stockByPart.TryGetValue(product.PartNumber, out var stockRow);
                levelByPart.TryGetValue(product.PartNumber, out var levelRow);

                foreach (var slot in selectedSlots)
                {
                    if (!TryParseSlot(slot.Code, out var slotIndex))
                        continue;

                    var stock = GetStockQty(stockRow, slotIndex);
                    var (min, max) = GetMinMax(levelRow, slotIndex);

                    var isOverstock = stock > max;
                    var isUnderstock = stock < min;

                    var wantsOver = request.OverstockOnly;
                    var wantsUnder = request.UnderstockOnly;
                    var matchesPosition = (!wantsOver && !wantsUnder)
                        || (wantsOver && isOverstock)
                        || (wantsUnder && isUnderstock);

                    if (!matchesPosition)
                        continue;

                    var difference = isOverstock
                        ? stock - max
                        : isUnderstock
                            ? stock - min
                            : 0;

                    results.Add(new StockPositionLineDto
                    {
                        PartNo = product.PartNumber,
                        CategoryA = product.CatA ?? product.CatACode ?? string.Empty,
                        CategoryB = product.CatB ?? product.CatBCode ?? string.Empty,
                        CategoryC = product.CatC ?? product.CatCCode ?? string.Empty,
                        Make = product.Make ?? string.Empty,
                        Size = product.Size ?? string.Empty,
                        Color = product.Color ?? string.Empty,
                        RRP = product.SuggestedRRP ?? product.StorePrice ?? 0m,
                        Promo = product.PromoPrice ?? 0m,
                        Location = slot.Code,
                        MIN = min,
                        MAX = max,
                        Stock = stock,
                        StockDifference = difference
                    });
                }
            }

            return results
                .OrderBy(x => x.Location, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.PartNo, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public async Task<List<MajorItemSalesReportLineDto>> GetMajorItemSalesAsync(MajorItemSalesReportRequestDto request)
        {
            var rawLocations = await _context._Locations
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => new
                {
                    x.Code,
                    x.Name
                })
                .ToListAsync();

            var locations = rawLocations
                .Select(x => new LocationSlot(NormalizeLocationCode(x.Code), x.Name))
                .Where(x => !string.IsNullOrWhiteSpace(x.Code) && TryParseSlot(x.Code, out var slot) && slot >= 1 && slot <= 30)
                .OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (locations.Count == 0)
                return new List<MajorItemSalesReportLineDto>();

            var selectedSlots = ResolveLocationRange(locations, request.FromLocation, request.ToLocation);
            if (selectedSlots.Count == 0)
                return new List<MajorItemSalesReportLineDto>();

            var dateBounds = NormalizeDateBounds(request.FromDate, request.ToDate);

            var anchorQuery = _context.ProductItems.AsNoTracking().Where(x => x.Major);

            if (!string.IsNullOrWhiteSpace(request.Make))
            {
                var make = request.Make.Trim();
                anchorQuery = anchorQuery.Where(x => x.Make == make);
            }

            if (!string.IsNullOrWhiteSpace(request.CategoryA))
            {
                var category = request.CategoryA.Trim();
                anchorQuery = anchorQuery.Where(x => x.CatACode == category || x.CatA == category);
            }

            if (!string.IsNullOrWhiteSpace(request.CategoryB))
            {
                var category = request.CategoryB.Trim();
                anchorQuery = anchorQuery.Where(x => x.CatBCode == category || x.CatB == category);
            }

            if (!string.IsNullOrWhiteSpace(request.CategoryC))
            {
                var category = request.CategoryC.Trim();
                anchorQuery = anchorQuery.Where(x => x.CatCCode == category || x.CatC == category);
            }

            var anchors = await anchorQuery
                .OrderBy(x => x.GroupCode ?? x.PartNumber)
                .ThenBy(x => x.PartNumber)
                .ToListAsync();

            if (anchors.Count == 0)
                return new List<MajorItemSalesReportLineDto>();

            var anchorPartNumbers = anchors
                .Select(x => x.PartNumber)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var groupKeys = anchors
                .Select(GetGroupKey)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var groupedMembers = await _context.ProductItems
                .AsNoTracking()
                .Where(x =>
                    anchorPartNumbers.Contains(x.PartNumber) ||
                    (x.GroupCode != null && groupKeys.Contains(x.GroupCode)))
                .ToListAsync();

            var groupedProducts = groupedMembers
                .GroupBy(GetGroupKey, StringComparer.OrdinalIgnoreCase)
                .Where(g => groupKeys.Any(key => string.Equals(key, g.Key, StringComparison.OrdinalIgnoreCase)))
                .Select(g => new
                {
                    Key = g.Key,
                    GroupName = g.Select(x => x.GroupName)
                        .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
                        ?? g.Key,
                    Cost = g.Select(x => x.CostPrice).FirstOrDefault(x => x.HasValue) ?? 0m,
                    PartNumbers = g.Select(x => x.PartNumber)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList()
                })
                .OrderBy(x => x.GroupName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (groupedProducts.Count == 0)
                return new List<MajorItemSalesReportLineDto>();

            var partNumbers = groupedProducts
                .SelectMany(x => x.PartNumbers)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var stockRows = await _context._ProductsStockLevels
                .AsNoTracking()
                .Where(x => partNumbers.Contains(x.PartNumber))
                .ToListAsync();

            var salesQuery = _context.FTT05
                .AsNoTracking()
                .Where(x =>
                    x.DateAndTime != null &&
                    x.DateAndTime >= dateBounds.From &&
                    x.DateAndTime <= dateBounds.To &&
                    partNumbers.Contains(x.PartNumber) &&
                    !string.IsNullOrWhiteSpace(x.InOut) &&
                    x.InOut.ToUpper().StartsWith("S"));

            var selectedLocationCodes = selectedSlots
                .Select(x => x.Code)
                .ToList();

            salesQuery = salesQuery.Where(x => selectedLocationCodes.Contains(x.Location));

            var salesRows = await salesQuery.ToListAsync();

            var stockByPart = stockRows
                .GroupBy(x => x.PartNumber, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.DateUpdated ?? x.DateCreated).First(),
                    StringComparer.OrdinalIgnoreCase);

            var salesByPart = salesRows
                .GroupBy(x => x.PartNumber, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            var results = new List<MajorItemSalesReportLineDto>();

            foreach (var group in groupedProducts)
            {
                var inStock = 0;
                var sold = 0;
                decimal soldValue = 0m;
                decimal turnover = 0m;

                foreach (var partNumber in group.PartNumbers)
                {
                    if (stockByPart.TryGetValue(partNumber, out var stockRow))
                    {
                        foreach (var slot in selectedSlots)
                        {
                            if (TryParseSlot(slot.Code, out var slotIndex))
                                inStock += GetStockQty(stockRow, slotIndex);
                        }
                    }

                    if (!salesByPart.TryGetValue(partNumber, out var partSales))
                        continue;

                    sold += partSales.Sum(x => x.Quantity);
                    soldValue += partSales.Sum(x => x.Quantity * x.Cost);
                    turnover += partSales.Sum(x => x.Quantity * x.Sell);
                }

                var profit = turnover - soldValue;
                var margin = turnover > 0m ? (profit / turnover) * 100m : 0m;

                results.Add(new MajorItemSalesReportLineDto
                {
                    Group = group.GroupName,
                    InStock = inStock,
                    InStockValue = inStock * group.Cost,
                    Sold = sold,
                    SoldValue = soldValue,
                    Profit = profit,
                    MarginPercent = margin,
                    Turnover = turnover
                });
            }

            return results
                .OrderBy(x => x.Group, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public async Task<List<MajorItemReportLineDto>> GetMajorItemReportAsync(MajorItemReportRequestDto request)
        {
            var rawLocations = await _context._Locations
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => new
                {
                    x.Code,
                    x.Name
                })
                .ToListAsync();

            var locations = rawLocations
                .Select(x => new LocationSlot(NormalizeLocationCode(x.Code), x.Name))
                .Where(x => !string.IsNullOrWhiteSpace(x.Code) && TryParseSlot(x.Code, out var slot) && slot >= 1 && slot <= 30)
                .OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (locations.Count == 0)
                return new List<MajorItemReportLineDto>();

            var selectedSlots = ResolveLocationRange(locations, request.FromLocation, request.ToLocation);
            if (selectedSlots.Count == 0)
                return new List<MajorItemReportLineDto>();

            var selectedLocationCodes = selectedSlots
                .Select(x => x.Code)
                .ToList();

            var categoryLevel = (request.CategoryLevel ?? string.Empty).Trim();
            var categoryCode = request.CategoryCode?.Trim();
            var headingMode = (request.HeadingMode ?? string.Empty).Trim();
            var headingValue = request.HeadingValue?.Trim();

            IQueryable<ProductItem> productQuery = _context.ProductItems
                .AsNoTracking()
                .Where(x => x.Major);

            if (!string.IsNullOrWhiteSpace(categoryCode))
            {
                switch (categoryLevel.ToUpperInvariant())
                {
                    case "A":
                        productQuery = productQuery.Where(x => x.CatACode == categoryCode || x.CatA == categoryCode);
                        break;
                    case "B":
                        productQuery = productQuery.Where(x => x.CatBCode == categoryCode || x.CatB == categoryCode);
                        break;
                    case "C":
                        productQuery = productQuery.Where(x => x.CatCCode == categoryCode || x.CatC == categoryCode);
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(headingValue))
            {
                switch (headingMode.ToUpperInvariant())
                {
                    case "MAKE":
                        productQuery = productQuery.Where(x => x.Make == headingValue);
                        break;
                    case "SUPPLIER":
                        productQuery = productQuery.Where(x => x.Supplier1Code == headingValue || x.Supplier2Code == headingValue);
                        break;
                    case "CATA":
                        productQuery = productQuery.Where(x => x.CatACode == headingValue || x.CatA == headingValue);
                        break;
                    case "CATB":
                        productQuery = productQuery.Where(x => x.CatBCode == headingValue || x.CatB == headingValue);
                        break;
                    case "CATC":
                        productQuery = productQuery.Where(x => x.CatCCode == headingValue || x.CatC == headingValue);
                        break;
                }
            }

            var products = await productQuery
                .OrderBy(x => x.PartNumber)
                .ToListAsync();

            if (products.Count == 0)
                return new List<MajorItemReportLineDto>();

            var partNumbers = products
                .Select(x => x.PartNumber)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (partNumbers.Count == 0)
                return new List<MajorItemReportLineDto>();

            var productByPart = products
                .GroupBy(x => x.PartNumber, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var stockRows = await _context._ProductsStock
                .AsNoTracking()
                .Where(x => partNumbers.Contains(x.PartNumber))
                .ToListAsync();

            var stockByPartAndNo = stockRows
                .Where(x => !string.IsNullOrWhiteSpace(x.StockNumber))
                .GroupBy(x => $"{x.PartNumber.Trim()}|{x.StockNumber.Trim()}", StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.DateCreated).First(), StringComparer.OrdinalIgnoreCase);

            var dateBounds = NormalizeDateBounds(request.FromDate, request.ToDate);
            var printBy = (request.PrintBy ?? string.Empty).Trim();

            if (printBy.Equals("Sold", StringComparison.OrdinalIgnoreCase))
            {
                var salesQuery = _context.FTT05
                    .AsNoTracking()
                    .Where(x =>
                        x.DateAndTime != null &&
                        x.DateAndTime >= dateBounds.From &&
                        x.DateAndTime <= dateBounds.To &&
                        selectedLocationCodes.Contains(x.Location) &&
                        partNumbers.Contains(x.PartNumber) &&
                        !string.IsNullOrWhiteSpace(x.InOut) &&
                        x.InOut.ToUpper().StartsWith("S"));

                var salesRows = await salesQuery
                    .OrderByDescending(x => x.DateAndTime)
                    .ThenBy(x => x.PartNumber)
                    .ThenBy(x => x.StockNo)
                    .ToListAsync();

                return salesRows
                    .Select(sale =>
                    {
                        productByPart.TryGetValue(sale.PartNumber.Trim(), out var product);
                        stockByPartAndNo.TryGetValue($"{sale.PartNumber.Trim()}|{sale.StockNo.Trim()}", out var stockRow);

                        return BuildMajorItemReportLine(
                            product,
                            stockRow,
                            sale.PartNumber,
                            sale.StockNo,
                            sale.DateAndTime ?? DateTime.MinValue,
                            sale.Cost,
                            sale.StockNo,
                            stockRow?.SerialNumber);
                    })
                    .ToList();
            }

            var stockQuery = _context._ProductsStock
                .AsNoTracking()
                .Where(x =>
                    partNumbers.Contains(x.PartNumber) &&
                    selectedLocationCodes.Contains(x.LocationCode) &&
                    x.DateCreated >= dateBounds.From &&
                    x.DateCreated <= dateBounds.To);

            var rows = await stockQuery
                .OrderByDescending(x => x.DateCreated)
                .ThenBy(x => x.PartNumber)
                .ThenBy(x => x.StockNumber)
                .ToListAsync();

            return rows
                .Select(stock =>
                {
                    productByPart.TryGetValue(stock.PartNumber.Trim(), out var product);
                    return BuildMajorItemReportLine(
                        product,
                        stock,
                        stock.PartNumber,
                        stock.StockNumber,
                        stock.DateCreated,
                        stock.Cost,
                        stock.StockNumber,
                        stock.SerialNumber);
                })
                .ToList();
        }

        public async Task<List<PriceListReportLineDto>> GetPriceListReportAsync(PriceListReportRequestDto request)
        {
            var rawLocations = await _context._Locations
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => new
                {
                    x.Code,
                    x.Name
                })
                .ToListAsync();

            var locations = rawLocations
                .Select(x => new LocationSlot(NormalizeLocationCode(x.Code), x.Name))
                .Where(x => !string.IsNullOrWhiteSpace(x.Code) && TryParseSlot(x.Code, out var slot) && slot >= 1 && slot <= 30)
                .OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (locations.Count == 0)
                return new List<PriceListReportLineDto>();

            var selectedSlots = ResolveLocationRange(locations, request.FromLocation, request.ToLocation);
            if (selectedSlots.Count == 0)
                return new List<PriceListReportLineDto>();

            var selectedLocationCodes = selectedSlots.Select(x => x.Code).ToList();

            IQueryable<ProductItem> productQuery = _context.ProductItems.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SupplierNo))
            {
                var supplierNo = request.SupplierNo.Trim();
                productQuery = productQuery.Where(x =>
                    x.Supplier1Code == supplierNo ||
                    x.Supplier2Code == supplierNo);
            }

            var productMode = (request.ProductMode ?? string.Empty).Trim();
            if (productMode.Equals("Major", StringComparison.OrdinalIgnoreCase))
                productQuery = productQuery.Where(x => x.Major);
            else if (productMode.Equals("Minor", StringComparison.OrdinalIgnoreCase))
                productQuery = productQuery.Where(x => !x.Major);

            if (!string.IsNullOrWhiteSpace(request.Make))
            {
                var make = request.Make.Trim();
                productQuery = productQuery.Where(x => x.Make == make);
            }

            if (!string.IsNullOrWhiteSpace(request.CategoryA))
            {
                var category = request.CategoryA.Trim();
                productQuery = productQuery.Where(x => x.CatACode == category || x.CatA == category);
            }

            if (!string.IsNullOrWhiteSpace(request.CategoryB))
            {
                var category = request.CategoryB.Trim();
                productQuery = productQuery.Where(x => x.CatBCode == category || x.CatB == category);
            }

            if (!string.IsNullOrWhiteSpace(request.CategoryC))
            {
                var category = request.CategoryC.Trim();
                productQuery = productQuery.Where(x => x.CatCCode == category || x.CatC == category);
            }

            if (!string.IsNullOrWhiteSpace(request.Year))
            {
                var year = request.Year.Trim();
                productQuery = productQuery.Where(x => x.Year == year);
            }

            if (request.PromoPriceOnly)
            {
                productQuery = productQuery.Where(x => (x.PromoPrice ?? 0m) > 0m);
            }

            var products = await productQuery
                .OrderBy(x => x.Make)
                .ThenBy(x => x.GroupName ?? x.ShortDescription ?? x.PartNumber)
                .ThenBy(x => x.PartNumber)
                .ToListAsync();

            if (request.NegativePriceOnly)
            {
                products = products
                    .Where(x => GetReportPrice(x) < 0m || (x.PromoPrice ?? 0m) < 0m)
                    .ToList();
            }

            if (products.Count == 0)
                return new List<PriceListReportLineDto>();

            var partNumbers = products
                .Select(x => x.PartNumber)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (partNumbers.Count == 0)
                return new List<PriceListReportLineDto>();

            var stockRows = await _context._ProductsStockLevels
                .AsNoTracking()
                .Where(x => partNumbers.Contains(x.PartNumber))
                .ToListAsync();

            var stockByPart = stockRows
                .GroupBy(x => x.PartNumber, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First(),
                    StringComparer.OrdinalIgnoreCase);

            var sortBy = (request.SortBy ?? string.Empty).Trim();
            var rows = products
                .Select(product =>
                {
                    stockByPart.TryGetValue(product.PartNumber, out var stockRow);
                    var totalStock = CalculateTotalStock(stockRow, selectedLocationCodes);
                    return new PriceListReportLineDto
                    {
                        Make = product.Make ?? string.Empty,
                        Model = product.GroupName ?? product.ShortDescription ?? product.PartNumber,
                        CatA = product.CatA ?? product.CatACode ?? string.Empty,
                        CatB = product.CatB ?? product.CatBCode ?? string.Empty,
                        CatC = product.CatC ?? product.CatCCode ?? string.Empty,
                        Year = product.Year ?? string.Empty,
                        PartNo = product.PartNumber,
                        Detail = product.Details ?? product.ShortDescription ?? string.Empty,
                        Size = product.Size ?? string.Empty,
                        Color = product.Color ?? string.Empty,
                        Barcode = product.Barcode ?? string.Empty,
                        MfrPartNumber = product.MfrPartNumber ?? product.MfrPartNumber2 ?? string.Empty,
                        TotalStock = totalStock,
                        Price = GetReportPrice(product),
                        PromoPrice = product.PromoPrice ?? 0m,
                        PromoStart = product.PromoStart,
                        PromoEnd = product.PromoEnd
                    };
                })
                .OrderBy(x => x.Make, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Model, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => GetSortValue(x, sortBy), StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.PartNo, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return rows;
        }

        public async Task<List<StockTransferReportLineDto>> GetStockTransferReportAsync(StockTransferReportRequestDto request)
        {
            var rawLocations = await _context._Locations
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => new
                {
                    x.Code,
                    x.Name
                })
                .ToListAsync();

            var locations = rawLocations
                .Select(x => new LocationSlot(NormalizeLocationCode(x.Code), x.Name))
                .Where(x => !string.IsNullOrWhiteSpace(x.Code) && TryParseSlot(x.Code, out var slot) && slot >= 1 && slot <= 30)
                .OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (locations.Count == 0)
                return new List<StockTransferReportLineDto>();

            var selectedSlots = ResolveLocationRange(locations, request.FromLocation, request.ToLocation);
            if (selectedSlots.Count == 0)
                return new List<StockTransferReportLineDto>();

            var selectedLocationCodes = selectedSlots
                .Select(x => x.Code)
                .ToList();

            var query = _context.ProductStockMovement
                .AsNoTracking()
                .Where(x =>
                    selectedLocationCodes.Contains(x.FromLocation) &&
                    selectedLocationCodes.Contains(x.ToLocation));

            var dateMode = (request.DateMode ?? string.Empty).Trim().ToLowerInvariant();
            var dateBounds = dateMode switch
            {
                "today" => NormalizeDateBounds(DateTime.Today, DateTime.Today),
                "daterange" => NormalizeDateBounds(request.FromDate, request.ToDate),
                _ => default
            };

            if (dateMode is "today" or "daterange")
            {
                query = query.Where(x =>
                    x.DateAndTime >= dateBounds.From &&
                    x.DateAndTime <= dateBounds.To);
            }

            var movementRows = await query
                .OrderByDescending(x => x.DateAndTime)
                .ThenBy(x => x.PartNo)
                .ToListAsync();

            if (movementRows.Count == 0)
                return new List<StockTransferReportLineDto>();

            var partNumbers = movementRows
                .Select(x => x.PartNo)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var products = await _context.ProductItems
                .AsNoTracking()
                .Where(x => partNumbers.Contains(x.PartNumber))
                .ToListAsync();

            var productByPart = products
                .GroupBy(x => x.PartNumber, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var rows = movementRows
                .Select(movement =>
                {
                    productByPart.TryGetValue(movement.PartNo, out var product);

                    return new StockTransferReportRow
                    {
                        DateAndTime = movement.DateAndTime,
                        PartNo = movement.PartNo,
                        MfrNo = product != null
                            ? (product.MfrPartNumber ?? product.MfrPartNumber2 ?? string.Empty)
                            : string.Empty,
                        LocFrom = movement.FromLocation,
                        LocTo = movement.ToLocation,
                        Cost = movement.Cost,
                        Qty = movement.StockQty,
                        Rrp = product != null
                            ? (product.SuggestedRRP ?? product.StorePrice ?? 0m)
                            : 0m,
                        StockNo = movement.StockNumber ?? string.Empty,
                        SerialNo = movement.SerialNumber ?? string.Empty,
                        Make = product?.Make ?? string.Empty,
                        Search1 = product?.Search1 ?? string.Empty,
                        Search2 = product?.Search2 ?? string.Empty,
                        Details = product?.Details ?? string.Empty,
                        Major = product?.Major == true,
                        CatA = product?.CatACode ?? product?.CatA,
                        CatB = product?.CatBCode ?? product?.CatB,
                        CatC = product?.CatCCode ?? product?.CatC
                    };
                })
                .ToList();

            var type = (request.Type ?? string.Empty).Trim().ToLowerInvariant();
            if (type is "major" or "minor")
                rows = rows.Where(x => type == "major" ? x.Major : !x.Major).ToList();

            if (!string.IsNullOrWhiteSpace(request.CategoryA))
            {
                var category = request.CategoryA.Trim();
                rows = rows.Where(x => string.Equals(x.CatA, category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(request.CategoryB))
            {
                var category = request.CategoryB.Trim();
                rows = rows.Where(x => string.Equals(x.CatB, category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(request.CategoryC))
            {
                var category = request.CategoryC.Trim();
                rows = rows.Where(x => string.Equals(x.CatC, category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return rows
                .OrderByDescending(x => x.DateAndTime)
                .ThenBy(x => x.PartNo, StringComparer.OrdinalIgnoreCase)
                .Select(x => new StockTransferReportLineDto
                {
                    DateAndTime = x.DateAndTime,
                    PartNo = x.PartNo,
                    MfrNo = x.MfrNo,
                    LocFrom = x.LocFrom,
                    LocTo = x.LocTo,
                    Cost = x.Cost,
                    Qty = x.Qty,
                    Rrp = x.Rrp,
                    StockNo = x.StockNo,
                    SerialNo = x.SerialNo,
                    Make = x.Make,
                    Search1 = x.Search1,
                    Search2 = x.Search2,
                    Details = x.Details
                })
                .ToList();
        }

        public async Task<List<LayawayReportLineDto>> GetLayawayReportAsync(LayawayReportRequestDto request)
        {
            var query = _context.Layaways
                .AsNoTracking()
                .AsQueryable();

            var fromLocation = NormalizeLocationCode(request.FromLocation);
            if (!string.IsNullOrWhiteSpace(fromLocation))
                query = query.Where(x => x.Location == fromLocation);

            if (request.CustomerMode?.Trim().Equals("One", StringComparison.OrdinalIgnoreCase) == true &&
                !string.IsNullOrWhiteSpace(request.CustomerAccNo))
            {
                var customerAcc = request.CustomerAccNo.Trim();
                query = query.Where(x => x.CustomerAccNo == customerAcc);
            }

            var layaways = await query
                .OrderByDescending(x => x.Date)
                .ThenBy(x => x.ReferenceNo)
                .ThenBy(x => x.PartNumber)
                .ToListAsync();

            if (layaways.Count == 0)
                return new List<LayawayReportLineDto>();

            var partNumbers = layaways
                .Select(x => x.PartNumber)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var customerAccs = layaways
                .Select(x => x.CustomerAccNo)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var products = await _context.ProductItems
                .AsNoTracking()
                .Where(x => partNumbers.Contains(x.PartNumber))
                .ToListAsync();

            var customers = await _context.Customers
                .AsNoTracking()
                .Where(x => customerAccs.Contains(x.AccNo))
                .ToListAsync();

            var stocks = await _context._ProductsStock
                .AsNoTracking()
                .Where(x => partNumbers.Contains(x.PartNumber))
                .ToListAsync();

            var productByPart = products
                .GroupBy(x => x.PartNumber, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var customerByAcc = customers
                .Where(x => !string.IsNullOrWhiteSpace(x.AccNo))
                .GroupBy(x => x.AccNo!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var stockByKey = stocks
                .Where(x => !string.IsNullOrWhiteSpace(x.PartNumber))
                .GroupBy(x => $"{x.PartNumber}|{x.LocationCode}|{x.StockNumber}", StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var rows = new List<LayawayReportLineDto>();

            foreach (var line in layaways)
            {
                productByPart.TryGetValue(line.PartNumber ?? string.Empty, out var product);
                customerByAcc.TryGetValue(line.CustomerAccNo ?? string.Empty, out var customer);

                var cost = ResolveLayawayCost(line, product, stockByKey);
                var qty = line.Quantity ?? 0;
                var sell = line.Sell ?? 0m;
                var major = product?.Major == true;
                var supplier = product?.Supplier1Code ?? product?.Supplier2Code ?? string.Empty;
                var partStockNo = string.IsNullOrWhiteSpace(line.StockNo)
                    ? line.PartNumber ?? string.Empty
                    : $"{line.PartNumber}/{line.StockNo}";

                rows.Add(new LayawayReportLineDto
                {
                    CustomerAcc = line.CustomerAccNo ?? string.Empty,
                    DateAndTime = line.Date ?? DateTime.MinValue,
                    Location = line.Location ?? string.Empty,
                    SalesCode = line.SalesPerson ?? string.Empty,
                    PartStockNo = partStockNo,
                    Mfr = product != null ? (product.MfrPartNumber ?? product.MfrPartNumber2 ?? string.Empty) : string.Empty,
                    Make = product?.Make ?? string.Empty,
                    Search1 = product?.Search1 ?? string.Empty,
                    Search2 = product?.Search2 ?? string.Empty,
                    CatA = product?.CatACode ?? product?.CatA ?? string.Empty,
                    CatB = product?.CatBCode ?? product?.CatB ?? string.Empty,
                    CatC = product?.CatCCode ?? product?.CatC ?? string.Empty,
                    Supplier = supplier,
                    Qty = qty,
                    Cost = cost,
                    TotalCost = cost * qty,
                    Price = sell,
                    TotalPrice = sell * qty,
                    Reserved = line.Reserved == true ? "Yes" : "No",
                    Type = major ? "Major" : "Minor",
                    RefIdJobNo = BuildLayawayRef(line),
                    DeliveryType = GetDeliveryTypeLabel(line.LayawayType)
                });
            }

            if (!string.IsNullOrWhiteSpace(request.CategoryA))
            {
                var value = request.CategoryA.Trim();
                rows = rows.Where(x => CategoryMatches(x.CatA, value)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(request.CategoryB))
            {
                var value = request.CategoryB.Trim();
                rows = rows.Where(x => CategoryMatches(x.CatB, value)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(request.CategoryC))
            {
                var value = request.CategoryC.Trim();
                rows = rows.Where(x => CategoryMatches(x.CatC, value)).ToList();
            }

            var type = (request.ProductType ?? string.Empty).Trim().ToLowerInvariant();
            if (type is "major" or "minor")
            {
                rows = rows.Where(x => type == "major"
                    ? string.Equals(x.Type, "Major", StringComparison.OrdinalIgnoreCase)
                    : string.Equals(x.Type, "Minor", StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return rows
                .OrderByDescending(x => x.DateAndTime)
                .ThenBy(x => x.CustomerAcc, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.PartStockNo, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string NormalizeLocationCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return string.Empty;

            var trimmed = code.Trim().ToUpperInvariant();
            return int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                ? value.ToString("D2", CultureInfo.InvariantCulture)
                : trimmed;
        }

        private static bool TryParseSlot(string code, out int slot)
            => int.TryParse(code, NumberStyles.Integer, CultureInfo.InvariantCulture, out slot);

        private static int GetStockQty(StockLevels? row, int slotIndex)
        {
            if (row == null)
                return 0;

            var prop = typeof(StockLevels).GetProperty($"L{slotIndex:00}", BindingFlags.Public | BindingFlags.Instance);
            return prop == null ? 0 : (int)(prop.GetValue(row) ?? 0);
        }

        private static (int Min, int Max) GetMinMax(ProductLevel? level, int slotIndex)
        {
            if (level == null)
                return (0, 0);

            var minProp = typeof(ProductLevel).GetProperty($"Min{slotIndex:00}", BindingFlags.Public | BindingFlags.Instance);
            var maxProp = typeof(ProductLevel).GetProperty($"Max{slotIndex:00}", BindingFlags.Public | BindingFlags.Instance);

            var min = minProp == null ? 0 : (int)(minProp.GetValue(level) ?? 0);
            var max = maxProp == null ? 0 : (int)(maxProp.GetValue(level) ?? 0);

            return (min, max);
        }

        private static (int FromYear, int ToYear)? NormalizeYearBounds(int? fromYear, int? toYear)
        {
            if (!fromYear.HasValue && !toYear.HasValue)
                return null;

            var from = fromYear ?? toYear!.Value;
            var to = toYear ?? fromYear!.Value;

            if (from > to)
                (from, to) = (to, from);

            return (from, to);
        }

        private static string GetGroupKey(ProductItem item)
            => !string.IsNullOrWhiteSpace(item.GroupCode)
                ? item.GroupCode.Trim()
                : item.PartNumber.Trim();

        private static bool CategoryMatches(string value, string category)
            => string.Equals(value, category, StringComparison.OrdinalIgnoreCase);

        private static string BuildLayawayRef(Layaway line)
        {
            var refNo = line.ReferenceNo?.ToString() ?? string.Empty;
            var jobNo = line.WorkshopJobNo ?? string.Empty;

            if (string.IsNullOrWhiteSpace(refNo))
                return jobNo;

            return string.IsNullOrWhiteSpace(jobNo) ? refNo : $"{refNo}/{jobNo}";
        }

        private static decimal ResolveLayawayCost(
            Layaway line,
            ProductItem? product,
            Dictionary<string, Stock> stockByKey)
        {
            if (!string.IsNullOrWhiteSpace(line.StockNo) &&
                !string.IsNullOrWhiteSpace(line.PartNumber) &&
                !string.IsNullOrWhiteSpace(line.Location))
            {
                var key = $"{line.PartNumber}|{line.Location}|{line.StockNo}";
                if (stockByKey.TryGetValue(key, out var stock))
                    return stock.Cost;
            }

            return product?.CostPrice ?? 0m;
        }

        private static decimal GetReportPrice(ProductItem product)
        {
            if (product.StorePrice.HasValue && product.StorePrice.Value != 0m)
                return product.StorePrice.Value;

            if (product.SuggestedRRP.HasValue && product.SuggestedRRP.Value != 0m)
                return product.SuggestedRRP.Value;

            if (product.TradePrice.HasValue && product.TradePrice.Value != 0m)
                return product.TradePrice.Value;

            if (product.WebPrice.HasValue && product.WebPrice.Value != 0m)
                return product.WebPrice.Value;

            return 0m;
        }

        private static int CalculateTotalStock(StockLevels? stockRow, List<string> selectedLocationCodes)
        {
            if (stockRow == null || selectedLocationCodes.Count == 0)
                return 0;

            var total = 0;

            foreach (var locationCode in selectedLocationCodes)
            {
                if (!TryParseSlot(locationCode, out var slotIndex))
                    continue;

                total += GetStockQty(stockRow, slotIndex);
            }

            return total;
        }

        private static string GetSortValue(PriceListReportLineDto row, string sortBy)
            => sortBy.ToUpperInvariant() switch
            {
                "MAKE" => row.Make,
                "MODEL" => row.Model,
                "MFR PART NUMBER" => row.MfrPartNumber,
                "DETAIL" => row.Detail,
                "CATEGORY" => $"{row.CatA}|{row.CatB}|{row.CatC}",
                _ => row.PartNo
            };

        private static string GetDeliveryTypeLabel(int? layawayType)
            => layawayType switch
            {
                0 => "Reserved",
                1 => "Sold",
                2 => "Reversed",
                null => string.Empty,
                _ => $"Type {layawayType}"
            };

        private static List<LocationSlot> ResolveLocationRange(
            List<LocationSlot> locations,
            string? fromLocation,
            string? toLocation)
        {
            var slots = locations
                .OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (slots.Count == 0)
                return new List<LocationSlot>();

            var fromCode = NormalizeLocationCode(fromLocation);
            var toCode = NormalizeLocationCode(toLocation);

            if (string.IsNullOrWhiteSpace(fromCode))
                fromCode = slots.First().Code;
            if (string.IsNullOrWhiteSpace(toCode))
                toCode = slots.Last().Code;

            if (string.CompareOrdinal(fromCode, toCode) > 0)
                (fromCode, toCode) = (toCode, fromCode);

            return slots
                .Where(x => string.CompareOrdinal(x.Code, fromCode) >= 0 &&
                            string.CompareOrdinal(x.Code, toCode) <= 0)
                .ToList();
        }

        private static (DateTime From, DateTime To) NormalizeDateBounds(DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate?.Date ?? DateTime.MinValue;
            var to = toDate?.Date.AddDays(1).AddTicks(-1) ?? DateTime.MaxValue;

            if (fromDate.HasValue && toDate.HasValue && from > to)
                (from, to) = (toDate.Value.Date, fromDate.Value.Date.AddDays(1).AddTicks(-1));

            return (from, to);
        }

        private static MajorItemReportLineDto BuildMajorItemReportLine(
            ProductItem? product,
            Stock? stock,
            string partNumber,
            string? stockNumber,
            DateTime dateIn,
            decimal cost,
            string? fallbackStockNo,
            string? fallbackFrameNumber)
        {
            var stockNo = !string.IsNullOrWhiteSpace(stockNumber)
                ? stockNumber.Trim()
                : !string.IsNullOrWhiteSpace(stock?.StockNumber)
                    ? stock.StockNumber.Trim()
                : !string.IsNullOrWhiteSpace(fallbackStockNo)
                    ? fallbackStockNo.Trim()
                    : string.Empty;

            var frameNumber = !string.IsNullOrWhiteSpace(stock?.SerialNumber)
                ? stock.SerialNumber.Trim()
                : !string.IsNullOrWhiteSpace(stock?.StockNumber)
                    ? stock.StockNumber.Trim()
                : !string.IsNullOrWhiteSpace(fallbackFrameNumber)
                    ? fallbackFrameNumber.Trim()
                    : stockNo;

            return new MajorItemReportLineDto
            {
                StockNo = stockNo,
                PartNo = partNumber,
                Make = product?.Make ?? string.Empty,
                Model = product?.GroupName ?? product?.ShortDescription ?? partNumber,
                Detail = product?.Details ?? product?.ShortDescription ?? string.Empty,
                Cost = cost,
                Rrp = product?.SuggestedRRP ?? product?.StorePrice ?? 0m,
                Promo = product?.PromoPrice ?? 0m,
                DateIn = dateIn,
                Size = product?.Size ?? string.Empty,
                Colour = product?.Color ?? string.Empty,
                Bin = ResolveBinLocation(product),
                FrameNumber = frameNumber,
                MfrSku = product?.MfrPartNumber ?? product?.MfrPartNumber2 ?? string.Empty
            };
        }

        private static string ResolveBinLocation(ProductItem? product)
        {
            if (product == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(product.BinLocation1))
                return product.BinLocation1.Trim();

            if (!string.IsNullOrWhiteSpace(product.BinLocation2))
                return product.BinLocation2.Trim();

            return string.Empty;
        }

        private sealed record LocationSlot(string Code, string? Name);

        private sealed class StockTransferReportRow : StockTransferReportLineDto
        {
            public bool Major { get; set; }
            public string? CatA { get; set; }
            public string? CatB { get; set; }
            public string? CatC { get; set; }
        }
    }
}

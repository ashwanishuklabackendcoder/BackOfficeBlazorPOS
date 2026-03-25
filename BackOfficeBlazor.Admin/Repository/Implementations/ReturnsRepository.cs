using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class ReturnsRepository : IReturnsRepository
    {
        private readonly BackOfficeAdminContext _context;
        private readonly IComboService _comboService;
        private readonly IStockMovementEngine _stockMovementEngine;

        public ReturnsRepository(
            BackOfficeAdminContext context,
            IComboService comboService,
            IStockMovementEngine stockMovementEngine)
        {
            _context = context;
            _comboService = comboService;
            _stockMovementEngine = stockMovementEngine;
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
                    (start, end) = (end, start);

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
            var saleRows = await _context.FTT05
                .Where(x => x.InvoiceNumber == invoiceNo && x.InOut == "O")
                .OrderBy(x => x.Id)
                .ToListAsync();

            if (saleRows.Count == 0)
                return new List<PosSaleLineDto>();

            var returnRows = await _context.FTT05
                .Where(x => x.InvoiceNumber == invoiceNo && x.InOut == "R")
                .OrderBy(x => x.Id)
                .ToListAsync();

            return await BuildReturnableSaleLinesAsync(saleRows, returnRows);
        }

        public async Task<PosReceiptDto?> GetReceiptAsync(string invoiceNo)
        {
            var rows = await _context.FTT05
                .Where(x => x.InvoiceNumber == invoiceNo && x.InOut == "O")
                .OrderBy(x => x.Id)
                .ToListAsync();

            if (rows.Count == 0)
                return null;

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

            var lines = await BuildSaleLinesAsync(rows);
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

        public async Task<List<ReturnHistoryDto>> GetReturnHistoryAsync(string invoiceNo)
        {
            if (string.IsNullOrWhiteSpace(invoiceNo))
                return new List<ReturnHistoryDto>();

            await EnsureReturnExtensionsSchemaAsync();

            var trackedEntities = await _context.ReturnItemTracking
                .AsNoTracking()
                .Where(x => x.InvoiceNo == invoiceNo)
                .OrderByDescending(x => x.ReturnDate)
                .ThenByDescending(x => x.Id)
                .ToListAsync();

            var trackedRows = trackedEntities
                .Select(x => new ReturnHistoryDto
                {
                    Id = x.Id,
                    InvoiceNo = x.InvoiceNo,
                    OriginalSaleLineId = x.OriginalSaleLineId,
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    ReturnedQty = x.Qty,
                    ReturnDate = x.ReturnDate,
                    ReturnCondition = x.Condition,
                    ReturnReason = x.Reason,
                    StockMovementStatus = x.StockMovementStatus
                })
                .ToList();

            var trackedReferenceIds = trackedEntities
                .Where(x => x.ReferenceReturnId.HasValue)
                .Select(x => x.ReferenceReturnId!.Value)
                .ToHashSet();

            var legacyRows = await _context.FTT05
                .AsNoTracking()
                .Where(x => x.InvoiceNumber == invoiceNo && x.InOut == "R")
                .OrderByDescending(x => x.DateAndTime)
                .ThenByDescending(x => x.Id)
                .ToListAsync();

            foreach (var row in legacyRows)
            {
                if (trackedReferenceIds.Contains(row.Id))
                    continue;

                trackedRows.Add(new ReturnHistoryDto
                {
                    Id = row.Id,
                    InvoiceNo = row.InvoiceNumber,
                    OriginalSaleLineId = row.OriginalSaleLineId,
                    ProductId = row.PartNumber,
                    ProductName = row.Description ?? string.Empty,
                    ReturnedQty = Math.Abs(row.Quantity),
                    ReturnDate = row.DateAndTime ?? DateTime.UtcNow,
                    ReturnCondition = "Legacy / Unknown",
                    ReturnReason = string.IsNullOrWhiteSpace(row.Notes) ? "Legacy return" : row.Notes,
                    StockMovementStatus = "Unknown (legacy return)"
                });
            }

            return trackedRows
                .OrderByDescending(x => x.ReturnDate)
                .ThenByDescending(x => x.Id)
                .ToList();
        }

        public async Task ProcessReturnAsync(ReturnProcessDto dto)
        {
            await EnsureReturnExtensionsSchemaAsync();
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                if (string.IsNullOrWhiteSpace(dto.InvoiceNo))
                    throw new Exception("Invoice number is required.");

                if (dto.Lines == null || dto.Lines.Count == 0)
                    throw new Exception("At least one return line is required.");

                if (dto.Lines.GroupBy(x => x.SaleLineId).Any(g => g.Count() > 1))
                    throw new Exception("Duplicate return entries are not allowed for the same sale line.");

                var customer = await GetCustomerByInvoiceAsync(dto.InvoiceNo);
                var invoiceLines = await GetInvoiceLinesAsync(dto.InvoiceNo);
                var saleRows = await _context.FTT05
                    .Where(x => x.InvoiceNumber == dto.InvoiceNo && x.InOut == "O")
                    .ToDictionaryAsync(x => x.Id);

                var sysOptions = await _context.SysOptions
                    .AsNoTracking()
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                var allowSeparateComboItemReturn = sysOptions?.AllowSeparateComboItemReturn ?? true;
                var returnStockMode = NormalizeReturnStockMode(sysOptions?.ReturnStockMode);
                var addSellableItemsToStock = returnStockMode switch
                {
                    ReturnStockModes.AlwaysAddToStock => true,
                    ReturnStockModes.NeverAddAutomatically => false,
                    _ => dto.AddSellableItemsToStock
                };

                foreach (var line in dto.Lines)
                {
                    var originalLine = invoiceLines.FirstOrDefault(x => x.SaleLineId == line.SaleLineId);

                    if (originalLine == null)
                        throw new Exception($"Original sale line not found for {line.PartNumber}.");

                    if (line.Qty <= 0)
                        throw new Exception($"Return quantity must be greater than zero for {originalLine.PartNumber}.");

                    if (line.Qty > originalLine.Quantity)
                        throw new Exception($"{originalLine.PartNumber} exceeds the remaining returnable quantity.");

                    if (string.IsNullOrWhiteSpace(line.Reason))
                        throw new Exception($"Return reason is required for {originalLine.PartNumber}.");

                    if (string.IsNullOrWhiteSpace(line.Condition))
                        throw new Exception($"Return condition is required for {originalLine.PartNumber}.");

                    line.PartNumber = originalLine.PartNumber;
                    line.Description = originalLine.Description;
                    line.StockNo = string.IsNullOrWhiteSpace(originalLine.StockNo) ? null : originalLine.StockNo;
                    line.Terminal = originalLine.Terminal;
                    line.Location = string.IsNullOrWhiteSpace(line.Location) ? originalLine.Location : line.Location;
                    line.Sell = originalLine.Sell;
                    line.IsCombo = originalLine.IsCombo;
                    line.ComboId = originalLine.ComboId;
                    line.ComboGroupId = originalLine.ComboGroupId;
                    line.IsComboReturnPolicyApplied = originalLine.IsComboReturnPolicyApplied;
                    line.RefundAmount = originalLine.Quantity <= 0
                        ? originalLine.RefundableAmount
                        : Math.Round((originalLine.RefundableAmount / originalLine.Quantity) * line.Qty, 2, MidpointRounding.AwayFromZero);
                }

                if (!allowSeparateComboItemReturn)
                {
                    var requestedComboGroups = dto.Lines
                        .Where(x => x.IsComboReturnPolicyApplied && !string.IsNullOrWhiteSpace(x.ComboGroupId))
                        .Select(x => x.ComboGroupId!)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    foreach (var comboGroupId in requestedComboGroups)
                    {
                        var originalGroupLines = invoiceLines
                            .Where(x => x.IsComboReturnPolicyApplied &&
                                        string.Equals(x.ComboGroupId, comboGroupId, StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        var selectedGroupLines = dto.Lines
                            .Where(x => string.Equals(x.ComboGroupId, comboGroupId, StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        if (selectedGroupLines.Count != originalGroupLines.Count)
                            throw new Exception("All items from the same combo sale must be returned together.");

                        foreach (var originalGroupLine in originalGroupLines)
                        {
                            var selectedGroupLine = selectedGroupLines.FirstOrDefault(x =>
                                x.SaleLineId == originalGroupLine.SaleLineId);

                            if (selectedGroupLine == null || selectedGroupLine.Qty != originalGroupLine.Quantity)
                                throw new Exception("All items from the same combo sale must be returned together.");
                        }
                    }
                }

                foreach (var line in dto.Lines)
                {
                    if (!saleRows.TryGetValue(line.SaleLineId, out var originalSaleRow))
                        throw new Exception($"Sale row metadata not found for {line.PartNumber}.");

                    var returnRow = await InsertReturnFtt05(dto, line, customer);
                    var stockMovementStatus = await HandleReturnStockAsync(line, addSellableItemsToStock);

                    await InsertReturnTrackingAsync(dto, line, originalSaleRow, returnRow.Id, stockMovementStatus);

                    if (!ReturnConditions.IsSellable(line.Condition))
                        await InsertFaultyReturnAsync(dto, line, originalSaleRow, customer, returnRow.Id);
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

        private async Task<List<PosSaleLineDto>> BuildSaleLinesAsync(List<FTT05> rows)
        {
            var makeLookup = await LoadMakeLookupAsync(rows);
            var lines = new List<PosSaleLineDto>();

            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.PartNumber))
                {
                    if (lines.Count > 0 && row.Net < 0)
                    {
                        var previousLine = lines[^1];
                        var discountTotal = Math.Abs(row.Net);
                        previousLine.DiscountAmount = previousLine.Quantity == 0
                            ? 0
                            : discountTotal / previousLine.Quantity;
                        previousLine.DiscountPercent = previousLine.Sell == 0
                            ? 0
                            : (previousLine.DiscountAmount / previousLine.Sell) * 100m;
                        previousLine.Net += row.Net;
                        previousLine.RefundableAmount = previousLine.Net;
                    }

                    continue;
                }

                var line = new PosSaleLineDto
                {
                    SaleLineId = row.Id,
                    PartNumber = row.PartNumber,
                    Description = row.Description ?? string.Empty,
                    StockNo = row.StockNo,
                    Quantity = row.Quantity,
                    Terminal = row.Terminal,
                    Location = row.Location,
                    Sell = row.Sell,
                    Net = row.Net,
                    Cost = row.Cost,
                    Vat = row.VAT,
                    IsMajor = !string.IsNullOrEmpty(row.StockNo),
                    IsCombo = row.IsCombo == true,
                    ComboId = row.ComboId,
                    ComboGroupId = string.IsNullOrWhiteSpace(row.ComboGroupId) ? null : row.ComboGroupId,
                    IsComboReturnPolicyApplied = row.IsComboReturnPolicyApplied == true,
                    RefundableAmount = row.Net,
                    Make = GetMake(makeLookup, row.PartNumber)
                };

                if (line.IsCombo && row.ComboId.HasValue && string.IsNullOrWhiteSpace(line.ComboGroupId))
                {
                    var combo = await _comboService.GetComboById(row.ComboId.Value);
                    if (combo.Success && combo.Data != null)
                    {
                        line.ComboItems = combo.Data.Details
                            .Select(x => new ComboInvoiceItemDto
                            {
                                PartNumber = x.PartNumber,
                                ProductName = x.ProductName ?? x.PartNumber,
                                Qty = x.Qty * row.Quantity
                            })
                            .ToList();
                    }
                }

                lines.Add(line);
            }

            return lines;
        }

        private async Task<List<PosSaleLineDto>> BuildReturnableSaleLinesAsync(List<FTT05> saleRows, List<FTT05> returnRows)
        {
            var saleLines = await BuildSaleLinesAsync(saleRows);
            if (saleLines.Count == 0)
                return saleLines;

            var indexedReturnedQuantities = returnRows
                .Where(x => x.OriginalSaleLineId.HasValue)
                .GroupBy(x => x.OriginalSaleLineId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => new ReturnAggregation
                    {
                        Quantity = g.Sum(x => Math.Abs(x.Quantity)),
                        Amount = g.Sum(x => Math.Abs(x.Net))
                    });

            var legacyReturnRows = returnRows
                .Where(x => !x.OriginalSaleLineId.HasValue)
                .ToList();

            var lines = new List<PosSaleLineDto>();

            foreach (var saleLine in saleLines)
            {
                var returnedQuantity = 0;
                var returnedAmount = 0m;

                if (indexedReturnedQuantities.TryGetValue(saleLine.SaleLineId, out var indexedReturn))
                {
                    returnedQuantity += indexedReturn.Quantity;
                    returnedAmount += indexedReturn.Amount;
                }

                if (legacyReturnRows.Count > 0)
                {
                    var matchingLegacyRows = legacyReturnRows.Where(x =>
                            string.Equals(x.PartNumber, saleLine.PartNumber, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(x.StockNo ?? string.Empty, saleLine.StockNo ?? string.Empty, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(x.ComboGroupId ?? string.Empty, saleLine.ComboGroupId ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    returnedQuantity += matchingLegacyRows.Sum(x => Math.Abs(x.Quantity));
                    returnedAmount += matchingLegacyRows.Sum(x => Math.Abs(x.Net));

                    if (matchingLegacyRows.Count > 0)
                        legacyReturnRows.RemoveAll(x => matchingLegacyRows.Contains(x));
                }

                var remainingQuantity = saleLine.Quantity - returnedQuantity;
                if (remainingQuantity <= 0)
                    continue;

                if (returnedQuantity > 0)
                {
                    saleLine.RefundableAmount = Math.Max(0m, saleLine.RefundableAmount - returnedAmount);
                    saleLine.Quantity = remainingQuantity;

                    if (saleLine.Quantity > 0 && saleLine.RefundableAmount > 0m)
                        saleLine.Net = saleLine.RefundableAmount;
                }

                lines.Add(saleLine);
            }

            return lines;
        }

        private async Task<FTT05> InsertReturnFtt05(ReturnProcessDto header, ReturnLineDto line, string customer)
        {
            var salesPersonCode = NormalizeSalesPersonCode(header.Staff);

            var entity = new FTT05
            {
                DateAndTime = DateTime.Now,
                Date = DateTime.Now.ToString("yyyyMMdd"),
                Time = DateTime.Now.ToString("HHmmss"),
                Location = line.Location,
                InvoiceNumber = header.InvoiceNo,
                PartNumber = line.PartNumber,
                Description = line.Description,
                ComboId = line.ComboId,
                IsCombo = line.IsCombo,
                ComboGroupId = line.ComboGroupId ?? string.Empty,
                IsComboReturnPolicyApplied = line.IsComboReturnPolicyApplied,
                OriginalSaleLineId = line.SaleLineId,
                StockNo = line.StockNo ?? string.Empty,
                Quantity = -line.Qty,
                Sell = -(line.RefundAmount <= 0 ? line.Sell : line.RefundAmount),
                Net = -(line.RefundAmount <= 0 ? line.Sell : line.RefundAmount),
                InOut = "R",
                Source = "RE",
                SalesPerson = salesPersonCode,
                Customer = customer,
                Terminal = line.Terminal
            };

            _context.FTT05.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        private async Task InsertReturnTrackingAsync(
            ReturnProcessDto header,
            ReturnLineDto line,
            FTT05 originalSaleRow,
            int referenceReturnId,
            string stockMovementStatus)
        {
            var entity = new ReturnItemTracking
            {
                InvoiceNo = header.InvoiceNo,
                OriginalSaleLineId = line.SaleLineId,
                ProductId = line.PartNumber,
                ProductName = line.Description,
                Qty = line.Qty,
                Reason = line.Reason.Trim(),
                Condition = line.Condition.Trim(),
                ReturnDate = DateTime.UtcNow,
                StockMovementStatus = stockMovementStatus,
                StoreId = NormalizeStoreId(line.Location),
                CreatedBy = (header.Staff ?? string.Empty).Trim(),
                ReferenceReturnId = referenceReturnId
            };

            _context.ReturnItemTracking.Add(entity);
            await _context.SaveChangesAsync();
        }

        private async Task InsertFaultyReturnAsync(
            ReturnProcessDto header,
            ReturnLineDto line,
            FTT05 originalSaleRow,
            string customer,
            int referenceReturnId)
        {
            var entity = new ReturnFaultyItem
            {
                InvoiceNo = header.InvoiceNo,
                ProductId = line.PartNumber,
                Qty = line.Qty,
                Reason = line.Reason.Trim(),
                Condition = line.Condition.Trim(),
                ReturnDate = DateTime.UtcNow,
                SaleDate = originalSaleRow.DateAndTime,
                SaleAmount = Math.Round(originalSaleRow.Sell * line.Qty, 2, MidpointRounding.AwayFromZero),
                ReturnAmount = Math.Round(line.RefundAmount, 2, MidpointRounding.AwayFromZero),
                DiscountAmount = CalculateDiscountAmount(originalSaleRow, line),
                CustomerAccount = customer,
                SalesCode = originalSaleRow.SalesPerson ?? string.Empty,
                StoreId = NormalizeStoreId(line.Location),
                CreatedBy = (header.Staff ?? string.Empty).Trim(),
                ReferenceReturnId = referenceReturnId
            };

            _context.ReturnFaultyItems.Add(entity);
            await _context.SaveChangesAsync();
        }

        private async Task<string> HandleReturnStockAsync(ReturnLineDto line, bool addSellableItemsToStock)
        {
            if (!ReturnConditions.IsSellable(line.Condition))
                return ReturnStockMovementStatuses.MovedToFaultyStock;

            if (!addSellableItemsToStock)
                return ReturnStockMovementStatuses.NotAddedToStock;

            var effectiveLocation = string.IsNullOrWhiteSpace(line.Location) ? line.Terminal : line.Location;

            if (line.IsCombo && string.IsNullOrWhiteSpace(line.ComboGroupId))
                return ReturnStockMovementStatuses.NotAddedToStock;

            if (!string.IsNullOrWhiteSpace(line.StockNo))
            {
                await _stockMovementEngine.ReturnMajorAsync(
                    line.PartNumber,
                    line.StockNo.Trim(),
                    effectiveLocation);

                return ReturnStockMovementStatuses.AddedToSellableStock;
            }

            await _stockMovementEngine.ReturnMinorAsync(
                line.PartNumber,
                line.Qty,
                effectiveLocation);

            return ReturnStockMovementStatuses.AddedToSellableStock;
        }

        private async Task InsertRefundFtt11(ReturnProcessDto dto)
        {
            var entity = new FTT11
            {
                Location = dto.Location,
                InvoiceNumber = dto.InvoiceNo,
                Amount = -dto.TotalRefund,
                Cash = -dto.RefundCash,
                Credit = -dto.RefundCredit,
                Type3Description = "Master Card",
                Type3 = -dto.RefundMasterCard,
                Type4Description = "Visa",
                Type4 = -dto.RefundVisa
            };

            _context.FTT11.Add(entity);
            await _context.SaveChangesAsync();
        }

        private async Task<string> GetCustomerByInvoiceAsync(string invoiceNo)
        {
            var customer = await _context.FTT05
                .Where(x => x.InvoiceNumber == invoiceNo && x.InOut == "O")
                .Select(x => x.Customer)
                .FirstOrDefaultAsync();

            return customer ?? string.Empty;
        }

        private static decimal CalculateDiscountAmount(FTT05 originalSaleRow, ReturnLineDto line)
        {
            if (originalSaleRow.Quantity <= 0)
                return 0m;

            var gross = originalSaleRow.Sell * originalSaleRow.Quantity;
            var net = originalSaleRow.Net;
            var discountTotal = Math.Max(0m, gross - net);
            if (discountTotal <= 0)
                return 0m;

            var unitDiscount = discountTotal / originalSaleRow.Quantity;
            return Math.Round(unitDiscount * line.Qty, 2, MidpointRounding.AwayFromZero);
        }

        private static string NormalizeReturnStockMode(string? mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
                return ReturnStockModes.AskUserEveryReturn;

            var normalized = mode.Trim().ToUpperInvariant();
            return normalized switch
            {
                ReturnStockModes.AlwaysAddToStock => ReturnStockModes.AlwaysAddToStock,
                ReturnStockModes.NeverAddAutomatically => ReturnStockModes.NeverAddAutomatically,
                _ => ReturnStockModes.AskUserEveryReturn
            };
        }

        private static string NormalizeStoreId(string? location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return "01";

            var trimmed = location.Trim();
            if (trimmed.Length == 1)
                return $"0{trimmed}";

            return trimmed.Length > 2
                ? trimmed[^2..].ToUpperInvariant()
                : trimmed.ToUpperInvariant();
        }

        private static string NormalizeSalesPersonCode(string? value)
        {
            var normalized = (value ?? string.Empty).Trim().ToUpperInvariant();
            if (normalized.Length <= 2)
                return normalized;

            return normalized[..2];
        }

        private Task EnsureReturnExtensionsSchemaAsync()
            => _context.Database.ExecuteSqlRawAsync(
                """
                IF COL_LENGTH('dbo.SysOptions', 'ReturnStockMode') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[SysOptions]
                        ADD [ReturnStockMode] NVARCHAR(40) NOT NULL
                            CONSTRAINT [DF_SysOptions_ReturnStockMode] DEFAULT('ASK_USER_EVERY_RETURN');
                END

                IF OBJECT_ID(N'dbo.ReturnItemTracking', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[ReturnItemTracking](
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [InvoiceNo] NVARCHAR(30) NOT NULL,
                        [OriginalSaleLineId] INT NULL,
                        [ProductId] NVARCHAR(20) NOT NULL,
                        [ProductName] NVARCHAR(500) NOT NULL CONSTRAINT [DF_ReturnItemTracking_ProductName] DEFAULT(''),
                        [Qty] INT NOT NULL,
                        [Reason] NVARCHAR(250) NOT NULL CONSTRAINT [DF_ReturnItemTracking_Reason] DEFAULT(''),
                        [Condition] NVARCHAR(50) NOT NULL CONSTRAINT [DF_ReturnItemTracking_Condition] DEFAULT('OK / Sellable'),
                        [ReturnDate] DATETIME2 NOT NULL CONSTRAINT [DF_ReturnItemTracking_ReturnDate] DEFAULT(SYSUTCDATETIME()),
                        [StockMovementStatus] NVARCHAR(60) NOT NULL CONSTRAINT [DF_ReturnItemTracking_StockMovementStatus] DEFAULT('Return recorded - not added to stock'),
                        [StoreId] NVARCHAR(2) NOT NULL CONSTRAINT [DF_ReturnItemTracking_StoreId] DEFAULT('01'),
                        [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_ReturnItemTracking_CreatedBy] DEFAULT(''),
                        [ReferenceReturnId] INT NULL
                    );

                    CREATE INDEX [IX_ReturnItemTracking_InvoiceNo_OriginalSaleLineId_ReferenceReturnId]
                        ON [dbo].[ReturnItemTracking]([InvoiceNo], [OriginalSaleLineId], [ReferenceReturnId]);
                END

                IF OBJECT_ID(N'dbo.ReturnItemTracking', N'U') IS NOT NULL
                   AND COL_LENGTH('dbo.ReturnItemTracking', 'InvoiceNo') < 60
                BEGIN
                    ALTER TABLE [dbo].[ReturnItemTracking] ALTER COLUMN [InvoiceNo] NVARCHAR(30) NOT NULL;
                END

                IF OBJECT_ID(N'dbo.ReturnFaultyItems', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[ReturnFaultyItems](
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [InvoiceNo] NVARCHAR(30) NOT NULL,
                        [ProductId] NVARCHAR(20) NOT NULL,
                        [Qty] INT NOT NULL,
                        [Reason] NVARCHAR(250) NOT NULL CONSTRAINT [DF_ReturnFaultyItems_Reason] DEFAULT(''),
                        [Condition] NVARCHAR(50) NOT NULL CONSTRAINT [DF_ReturnFaultyItems_Condition] DEFAULT('Faulty'),
                        [ReturnDate] DATETIME2 NOT NULL CONSTRAINT [DF_ReturnFaultyItems_ReturnDate] DEFAULT(SYSUTCDATETIME()),
                        [SaleDate] DATETIME2 NULL,
                        [SaleAmount] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_ReturnFaultyItems_SaleAmount] DEFAULT(0),
                        [ReturnAmount] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_ReturnFaultyItems_ReturnAmount] DEFAULT(0),
                        [DiscountAmount] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_ReturnFaultyItems_DiscountAmount] DEFAULT(0),
                        [CustomerAccount] NVARCHAR(20) NOT NULL CONSTRAINT [DF_ReturnFaultyItems_CustomerAccount] DEFAULT(''),
                        [SalesCode] NVARCHAR(20) NOT NULL CONSTRAINT [DF_ReturnFaultyItems_SalesCode] DEFAULT(''),
                        [StoreId] NVARCHAR(2) NOT NULL CONSTRAINT [DF_ReturnFaultyItems_StoreId] DEFAULT('01'),
                        [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_ReturnFaultyItems_CreatedBy] DEFAULT(''),
                        [ReferenceReturnId] INT NULL
                    );

                    CREATE INDEX [IX_ReturnFaultyItems_InvoiceNo_ProductId_ReferenceReturnId]
                        ON [dbo].[ReturnFaultyItems]([InvoiceNo], [ProductId], [ReferenceReturnId]);
                END

                IF OBJECT_ID(N'dbo.ReturnFaultyItems', N'U') IS NOT NULL
                   AND COL_LENGTH('dbo.ReturnFaultyItems', 'InvoiceNo') < 60
                BEGIN
                    ALTER TABLE [dbo].[ReturnFaultyItems] ALTER COLUMN [InvoiceNo] NVARCHAR(30) NOT NULL;
                END
                """);

        private sealed class ReturnAggregation
        {
            public int Quantity { get; set; }
            public decimal Amount { get; set; }
        }
    }
}

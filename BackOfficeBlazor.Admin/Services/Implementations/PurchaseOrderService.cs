using System.Text.Json;
using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Admin.Services;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private const string DraftOrderNumber = "0000000000";

        private readonly BackOfficeAdminContext _context;
        private readonly IProductRepository _productRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IStockLevelRepository _stockLevelRepository;
        private readonly IProductStockRepository _productStockRepository;

        public PurchaseOrderService(
            BackOfficeAdminContext context,
            IProductRepository productRepository,
            ISupplierRepository supplierRepository,
            IStockLevelRepository stockLevelRepository,
            IProductStockRepository productStockRepository)
        {
            _context = context;
            _productRepository = productRepository;
            _supplierRepository = supplierRepository;
            _stockLevelRepository = stockLevelRepository;
            _productStockRepository = productStockRepository;
        }

        public async Task<ApiResponse<PurchaseOrderWorkspaceDto>> SaveDraftAsync(PurchaseOrderUpsertRequestDto request)
        {
            try
            {
                var workspace = await SaveDraftInternalAsync(request);
                return ApiResponse<PurchaseOrderWorkspaceDto>.Ok(workspace, "Draft saved");
            }
            catch (Exception ex)
            {
                return ApiResponse<PurchaseOrderWorkspaceDto>.Fail($"Failed to save draft: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PurchaseOrderWorkspaceDto>> RaiseAsync(PurchaseOrderUpsertRequestDto request)
        {
            try
            {
                var draft = await SaveDraftInternalAsync(request);
                var result = await RaiseInternalAsync(draft.DraftRef, request);
                return ApiResponse<PurchaseOrderWorkspaceDto>.Ok(result, "Purchase order raised");
            }
            catch (Exception ex)
            {
                return ApiResponse<PurchaseOrderWorkspaceDto>.Fail($"Failed to raise order: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PurchaseOrderWorkspaceDto>> RaiseDirectAsync(PurchaseOrderDirectRaiseRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.SupplierCode))
                    return ApiResponse<PurchaseOrderWorkspaceDto>.Fail("Supplier is required");

                var supplierCode = request.SupplierCode.Trim().ToUpperInvariant();
                var locationCode = string.IsNullOrWhiteSpace(request.LocationCode)
                    ? string.Empty
                    : NormalizeLocation(request.LocationCode);

                var draftItemsQuery = _context._PurchaseOrderItems
                    .Where(x =>
                        x.OrderNumber == DraftOrderNumber &&
                        x.SupplierCode == supplierCode &&
                        x.QtyRequired >= 1);

                if (!string.IsNullOrWhiteSpace(locationCode))
                    draftItemsQuery = draftItemsQuery.Where(x => x.StockLocationCode == locationCode);

                if (request.ItemScope == PurchaseOrderItemScope.MajorOnly)
                    draftItemsQuery = draftItemsQuery.Where(x => x.IsMajor);
                else if (request.ItemScope == PurchaseOrderItemScope.MinorOnly)
                    draftItemsQuery = draftItemsQuery.Where(x => !x.IsMajor);

                var draftItems = await draftItemsQuery
                    .OrderBy(x => x.StockLocationCode)
                    .ThenBy(x => x.PartNumber)
                    .ThenBy(x => x.SequenceId)
                    .ToListAsync();

                if (!draftItems.Any())
                    return ApiResponse<PurchaseOrderWorkspaceDto>.Fail("No reorder items found for the selected supplier and location");

                using var tx = await _context.Database.BeginTransactionAsync();

                var orderNumber = await GenerateNextOrderNumberAsync();
                var now = DateTime.UtcNow;
                var header = new PurchaseOrderHeader
                {
                    OrderNumber = orderNumber,
                    RaisedByStaffCode = Truncate(request.RaisedByStaffCode?.Trim().ToUpperInvariant(), 2),
                    RaisedOnDate = now,
                    CarriageCost = 0,
                    Status = (int)PurchaseOrderStatus.Raised,
                    SupplierCode = supplierCode,
                    IsImported = false,
                    DateCreated = now,
                    DirectToStore = false
                };

                await _context._PurchaseOrderHeaders.AddAsync(header);

                foreach (var item in draftItems)
                {
                    item.OrderNumber = orderNumber;
                    item.DateUpdated = now;
                }

                var supplier = await _supplierRepository.GetByAccountNoAsync(supplierCode);
                header.JsonReport = BuildJsonReport(
                    header,
                    draftItems,
                    supplier,
                    string.IsNullOrWhiteSpace(locationCode) ? "ALL LOC" : locationCode,
                    request.FooterMessage);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                var raised = BuildWorkspace(header, draftItems, null, supplier);
                return ApiResponse<PurchaseOrderWorkspaceDto>.Ok(
                    raised,
                    request.PreviewOnly ? "Purchase order raised and ready for preview" : "Purchase order raised");
            }
            catch (Exception ex)
            {
                return ApiResponse<PurchaseOrderWorkspaceDto>.Fail($"Failed to raise order: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PurchaseOrderWorkspaceDto>> GetDraftAsync(int draftRef)
        {
            try
            {
                var items = await _context._PurchaseOrderItems
                    .AsNoTracking()
                    .Where(x => x.OrderNumber == DraftOrderNumber && x.InternalOrderRefID == draftRef)
                    .OrderBy(x => x.SequenceId)
                    .ToListAsync();

                if (!items.Any())
                    return ApiResponse<PurchaseOrderWorkspaceDto>.Fail("Draft not found");

                var supplierCode = items.First().SupplierCode;
                var supplier = await _supplierRepository.GetByAccountNoAsync(supplierCode);
                return ApiResponse<PurchaseOrderWorkspaceDto>.Ok(
                    BuildWorkspace(null, items, draftRef, supplier),
                    "Draft loaded");
            }
            catch (Exception ex)
            {
                return ApiResponse<PurchaseOrderWorkspaceDto>.Fail($"Failed to load draft: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PurchaseOrderWorkspaceDto>> GetAsync(string orderNumber)
        {
            try
            {
                orderNumber = NormalizeOrderNumber(orderNumber);
                var header = await _context._PurchaseOrderHeaders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.OrderNumber == orderNumber);

                if (header == null)
                    return ApiResponse<PurchaseOrderWorkspaceDto>.Fail("Purchase order not found");

                var items = await _context._PurchaseOrderItems
                    .AsNoTracking()
                    .Where(x => x.OrderNumber == orderNumber)
                    .OrderBy(x => x.SequenceId)
                    .ToListAsync();

                var supplier = await _supplierRepository.GetByAccountNoAsync(header.SupplierCode);
                return ApiResponse<PurchaseOrderWorkspaceDto>.Ok(
                    BuildWorkspace(header, items, null, supplier),
                    "Purchase order loaded");
            }
            catch (Exception ex)
            {
                return ApiResponse<PurchaseOrderWorkspaceDto>.Fail($"Failed to load order: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<PurchaseOrderSummaryDto>>> SearchAsync(string? query, string? supplierCode, int? status)
        {
            try
            {
                var q = _context._PurchaseOrderHeaders.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(query))
                {
                    var term = query.Trim();
                    q = q.Where(x => x.OrderNumber.Contains(term) || x.SupplierCode.Contains(term));
                }

                if (!string.IsNullOrWhiteSpace(supplierCode))
                {
                    supplierCode = supplierCode.Trim();
                    q = q.Where(x => x.SupplierCode == supplierCode);
                }
                if (status.HasValue)
                    q = q.Where(x => x.Status == status.Value);

                var headers = await q
                    .OrderByDescending(x => x.RaisedOnDate)
                    .Take(100)
                    .ToListAsync();

                var result = new List<PurchaseOrderSummaryDto>();
                foreach (var header in headers)
                {
                    var supplier = await _supplierRepository.GetByAccountNoAsync(header.SupplierCode);
                    var items = await _context._PurchaseOrderItems
                        .AsNoTracking()
                        .Where(x => x.OrderNumber == header.OrderNumber)
                        .ToListAsync();

                    result.Add(new PurchaseOrderSummaryDto
                    {
                        OrderNumber = header.OrderNumber,
                        SupplierCode = header.SupplierCode,
                        SupplierName = supplier?.Name ?? string.Empty,
                        RaisedOnDate = header.RaisedOnDate,
                        Status = StatusText(header.Status),
                        LineCount = items.Count,
                        QtyOrdered = items.Sum(x => x.QtyRequired),
                        QtyReceived = items.Sum(x => x.QtyRecieved),
                        CarriageCost = (decimal)header.CarriageCost,
                        TotalCost = items.Sum(x => x.QtyRequired * x.CostPrice)
                    });
                }

                return ApiResponse<List<PurchaseOrderSummaryDto>>.Ok(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<PurchaseOrderSummaryDto>>.Fail($"Search failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<PurchaseOrderSupplierOptionDto>>> GetSupplierOptionsAsync(int? status)
        {
            try
            {
                var q = _context._PurchaseOrderHeaders.AsNoTracking().AsQueryable();

                if (status.HasValue)
                    q = q.Where(x => x.Status == status.Value);

                var headers = await q
                    .Select(x => x.SupplierCode)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToListAsync();

                if (!headers.Any())
                    return ApiResponse<List<PurchaseOrderSupplierOptionDto>>.Ok(new List<PurchaseOrderSupplierOptionDto>());

                var result = new List<PurchaseOrderSupplierOptionDto>();
                foreach (var supplierCode in headers)
                {
                    var supplier = await _supplierRepository.GetByAccountNoAsync(supplierCode);
                    result.Add(new PurchaseOrderSupplierOptionDto
                    {
                        SupplierCode = supplierCode,
                        SupplierName = supplier?.Name ?? string.Empty
                    });
                }

                return ApiResponse<List<PurchaseOrderSupplierOptionDto>>.Ok(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<PurchaseOrderSupplierOptionDto>>.Fail($"Supplier search failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PurchaseOrderWorkspaceDto>> AmendAsync(PurchaseOrderAmendRequestDto request)
        {
            try
            {
                var orderNumber = NormalizeOrderNumber(request.OrderNumber);
                var header = await _context._PurchaseOrderHeaders
                    .FirstOrDefaultAsync(x => x.OrderNumber == orderNumber);

                if (header == null)
                    return ApiResponse<PurchaseOrderWorkspaceDto>.Fail("Purchase order not found");

                if (header.Status != (int)PurchaseOrderStatus.Raised)
                    return ApiResponse<PurchaseOrderWorkspaceDto>.Fail("Only raised purchase orders can be amended");

                var items = await _context._PurchaseOrderItems
                    .Where(x => x.OrderNumber == orderNumber)
                    .OrderBy(x => x.SequenceId)
                    .ToListAsync();

                if (!items.Any())
                    return ApiResponse<PurchaseOrderWorkspaceDto>.Fail("Purchase order items not found");

                if (request.Lines.Count == 0)
                    return ApiResponse<PurchaseOrderWorkspaceDto>.Fail("At least one purchase order line is required");

                using var tx = await _context.Database.BeginTransactionAsync();

                var amendedByCode = Truncate(request.AmendedByCode?.Trim().ToUpperInvariant(), 2);
                var now = DateTime.UtcNow;

                foreach (var item in items)
                {
                    var amendLine = request.Lines.FirstOrDefault(x =>
                        x.ItemId == item.Id ||
                        (x.SequenceId > 0 && x.SequenceId == item.SequenceId) ||
                        (!string.IsNullOrWhiteSpace(x.PartNumber) &&
                         string.Equals(x.PartNumber.Trim(), item.PartNumber, StringComparison.OrdinalIgnoreCase)));

                    if (amendLine == null)
                        continue;

                    if (amendLine.QtyRequired <= 0)
                        return ApiResponse<PurchaseOrderWorkspaceDto>.Fail("Qty Required must be greater than zero");

                    item.QtyRequired = amendLine.QtyRequired;
                    item.CostPrice = amendLine.CostPrice;
                    item.DateUpdated = now;
                }

                header.AmendedLastByCode = amendedByCode;
                header.AmendedLastOnDate = now;
                header.DateUpdated = now;
                header.JsonReport = BuildJsonReport(
                    header,
                    items,
                    await _supplierRepository.GetByAccountNoAsync(header.SupplierCode),
                    items.FirstOrDefault()?.StockLocationCode,
                    string.Empty);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                var supplier = await _supplierRepository.GetByAccountNoAsync(header.SupplierCode);
                return ApiResponse<PurchaseOrderWorkspaceDto>.Ok(
                    BuildWorkspace(header, items, null, supplier),
                    "Purchase order amended");
            }
            catch (DbUpdateException ex)
            {
                return ApiResponse<PurchaseOrderWorkspaceDto>.Fail($"Failed to amend order: {ex.GetBaseException().Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse<PurchaseOrderWorkspaceDto>.Fail($"Failed to amend order: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PurchaseOrderWorkspaceDto>> ReceiveAsync(PurchaseOrderReceiveRequestDto request)
        {
            try
            {
                var orderNumber = NormalizeOrderNumber(request.OrderNumber);
                var header = await _context._PurchaseOrderHeaders
                    .FirstOrDefaultAsync(x => x.OrderNumber == orderNumber);

                if (header == null)
                    return ApiResponse<PurchaseOrderWorkspaceDto>.Fail("Purchase order not found");

                if (header.Status == (int)PurchaseOrderStatus.Cancelled)
                    return ApiResponse<PurchaseOrderWorkspaceDto>.Fail("Purchase order is cancelled");

                var items = await _context._PurchaseOrderItems
                    .Where(x => x.OrderNumber == orderNumber)
                    .OrderBy(x => x.SequenceId)
                    .ToListAsync();

                if (!items.Any())
                    return ApiResponse<PurchaseOrderWorkspaceDto>.Fail("Purchase order items not found");

                using var tx = await _context.Database.BeginTransactionAsync();

                try
                {
                    var receiptStaffCode = Truncate(request.ReceivedByCode?.Trim().ToUpperInvariant(), 2);
                    var totalOrderedQty = items.Sum(x => Math.Max(x.QtyRequired, 0));
                    var perUnitAdjustment = 0m;
                    if (totalOrderedQty > 0)
                    {
                        if (request.DivideCarriageAcrossItems)
                            perUnitAdjustment += request.CarriageCost / totalOrderedQty;

                        if (request.DivideShippingAcrossItems)
                            perUnitAdjustment += request.ShippingCost / totalOrderedQty;

                        if (request.DivideSettlementDiscountAcrossItems)
                            perUnitAdjustment -= request.SettlementDiscount / totalOrderedQty;
                    }

                    foreach (var item in items)
                    {
                        var receivedLine = request.Lines.FirstOrDefault(x =>
                            x.ItemId == item.Id ||
                            (x.SequenceId > 0 && x.SequenceId == item.SequenceId) ||
                            (!string.IsNullOrWhiteSpace(x.PartNumber) &&
                             string.Equals(x.PartNumber.Trim(), item.PartNumber, StringComparison.OrdinalIgnoreCase)));

                        var remaining = Math.Max(item.QtyRequired - item.QtyRecieved, 0);
                        var receivedQty = receivedLine != null
                            ? Math.Clamp(receivedLine.QtyReceived, 0, remaining)
                            : remaining;

                        if (receivedQty <= 0)
                            continue;

                        var lineUnitCost = decimal.Round(
                            (receivedLine?.ReceivedUnitCost ?? item.CostPrice) + perUnitAdjustment,
                            2,
                            MidpointRounding.AwayFromZero);

                        var location = NormalizeLocation(
                            receivedLine?.StockLocationCode ??
                            item.StockLocationCode ??
                            item.DeliveryLocationCode);

                        await _stockLevelRepository.EnsureExistsAsync(item.PartNumber);
                        await _stockLevelRepository.IncrementAsync(item.PartNumber, location, receivedQty);

                        if (item.IsMajor)
                        {
                            for (var i = 0; i < receivedQty; i++)
                            {
                                await _productStockRepository.InsertAsync(new Stock
                                {
                                    PartNumber = item.PartNumber,
                                    LocationCode = location,
                                    Quantity = 1,
                                    Cost = lineUnitCost,
                                    StockNumber = StockInputService.GenerateStockNumber(10),
                                    CustomerAccNo = Truncate(string.IsNullOrWhiteSpace(item.CustomerAccNo) ? "000000" : item.CustomerAccNo, 6),
                                    SerialNumber = Truncate("N/A", 10),
                                    SupplierCode = Truncate(header.SupplierCode, 6),
                                    InvoiceNumber = Truncate(orderNumber, 15),
                                    PurchaseOrderNo = Truncate(orderNumber, 15),
                                    PrintLabelOption = 0,
                                    IsPrinted = false,
                                    IsCollected = false,
                                    IsAvailable = true,
                                    DateCreated = DateTime.UtcNow,
                                    StaffCode = receiptStaffCode
                                });
                            }
                        }
                        else
                        {
                            await _productStockRepository.InsertAsync(new Stock
                            {
                                PartNumber = item.PartNumber,
                                LocationCode = location,
                                Quantity = receivedQty,
                                Cost = lineUnitCost,
                                StockNumber = "0000000000",
                                CustomerAccNo = Truncate(string.IsNullOrWhiteSpace(item.CustomerAccNo) ? "000000" : item.CustomerAccNo, 6),
                                SerialNumber = Truncate("N/A", 10),
                                SupplierCode = Truncate(header.SupplierCode, 6),
                                InvoiceNumber = Truncate(orderNumber, 15),
                                PurchaseOrderNo = Truncate(orderNumber, 15),
                                PrintLabelOption = 0,
                                IsPrinted = false,
                                IsCollected = false,
                                IsAvailable = true,
                                DateCreated = DateTime.UtcNow,
                                StaffCode = receiptStaffCode
                            });
                        }

                        if (request.UpdateProductItemPrices)
                        {
                            var product = await _productRepository.GetByPartNumberAsync(item.PartNumber);
                            if (product != null)
                            {
                                product.CostPrice = lineUnitCost;
                                product.UpdatedOn = DateTime.UtcNow;
                                await _productRepository.UpdateAsync(product);
                            }
                        }

                        item.QtyRecieved += receivedQty;
                        item.CostPrice = lineUnitCost;
                        item.DateUpdated = DateTime.UtcNow;
                    }

                    header.Status = items.All(x => x.QtyRecieved >= x.QtyRequired)
                        ? (int)PurchaseOrderStatus.Received
                        : (int)PurchaseOrderStatus.PartReceived;

                    if (header.Status == (int)PurchaseOrderStatus.Received)
                    {
                        header.ClosedByCode = receiptStaffCode;
                        header.ClosedOnDate = DateTime.UtcNow;
                    }

                    header.DateUpdated = DateTime.UtcNow;
                    header.JsonReport = BuildJsonReport(
                        header,
                        items,
                        await _supplierRepository.GetByAccountNoAsync(header.SupplierCode),
                        items.FirstOrDefault()?.StockLocationCode,
                        string.Empty);

                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();

                    return ApiResponse<PurchaseOrderWorkspaceDto>.Ok(
                        BuildWorkspace(header, items, null, await _supplierRepository.GetByAccountNoAsync(header.SupplierCode)),
                        "Purchase order received");
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }
            catch (DbUpdateException ex)
            {
                return ApiResponse<PurchaseOrderWorkspaceDto>.Fail($"Failed to receive order: {ex.GetBaseException().Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse<PurchaseOrderWorkspaceDto>.Fail($"Failed to receive order: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> CancelAsync(string orderNumber, string cancelledByCode)
        {
            try
            {
                orderNumber = NormalizeOrderNumber(orderNumber);
                var header = await _context._PurchaseOrderHeaders.FirstOrDefaultAsync(x => x.OrderNumber == orderNumber);

                if (header == null)
                    return ApiResponse<bool>.Fail("Purchase order not found");

                header.Status = (int)PurchaseOrderStatus.Cancelled;
                header.CancelledByCode = cancelledByCode.Length <= 2 ? cancelledByCode : cancelledByCode[..2];
                header.CancelledOnDate = DateTime.UtcNow;
                header.DateUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ApiResponse<bool>.Ok(true, "Purchase order cancelled");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Cancel failed: {ex.Message}");
            }
        }

        private async Task<PurchaseOrderWorkspaceDto> SaveDraftInternalAsync(PurchaseOrderUpsertRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.SupplierCode))
                throw new InvalidOperationException("Supplier is required");

            if (request.Lines.Count == 0)
                throw new InvalidOperationException("At least one purchase order line is required");

            var draftRef = request.DraftRef ?? await GetNextDraftRefAsync();

            using var tx = await _context.Database.BeginTransactionAsync();

            var existing = await _context._PurchaseOrderItems
                .Where(x => x.OrderNumber == DraftOrderNumber && x.InternalOrderRefID == draftRef)
                .ToListAsync();

            if (existing.Any())
                _context._PurchaseOrderItems.RemoveRange(existing);

            var normalizedSupplier = request.SupplierCode.Trim().ToUpperInvariant();
            var lines = await BuildLinesAsync(request.Lines, normalizedSupplier, draftRef, request.RaisedByStaffCode);

            await _context._PurchaseOrderItems.AddRangeAsync(lines);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            var supplier = await _supplierRepository.GetByAccountNoAsync(normalizedSupplier);
            return BuildWorkspace(null, lines, draftRef, supplier);
        }

        private async Task<PurchaseOrderWorkspaceDto> RaiseInternalAsync(
            int draftRef,
            PurchaseOrderUpsertRequestDto request,
            string? reportLocationCode = null,
            string footerMessage = "")
        {
            var draftItems = await _context._PurchaseOrderItems
                .Where(x => x.OrderNumber == DraftOrderNumber && x.InternalOrderRefID == draftRef)
                .OrderBy(x => x.SequenceId)
                .ToListAsync();

            if (!draftItems.Any())
                throw new InvalidOperationException("Draft items not found");

            var orderNumber = await GenerateNextOrderNumberAsync();
            var now = DateTime.UtcNow;
            var header = new PurchaseOrderHeader
            {
                OrderNumber = orderNumber,
                RaisedByStaffCode = request.RaisedByStaffCode.Trim(),
                RaisedOnDate = now,
                CarriageCost = (double)request.CarriageCost,
                Status = (int)PurchaseOrderStatus.Raised,
                SupplierCode = request.SupplierCode.Trim().ToUpperInvariant(),
                IsImported = false,
                DateCreated = now,
                DirectToStore = request.DirectToStore
            };

            await _context._PurchaseOrderHeaders.AddAsync(header);

            foreach (var item in draftItems)
            {
                item.OrderNumber = orderNumber;
                item.DateUpdated = now;
            }

            header.JsonReport = BuildJsonReport(
                header,
                draftItems,
                await _supplierRepository.GetByAccountNoAsync(header.SupplierCode),
                reportLocationCode ?? draftItems.FirstOrDefault()?.StockLocationCode,
                footerMessage);
            await _context.SaveChangesAsync();

            var supplier = await _supplierRepository.GetByAccountNoAsync(header.SupplierCode);
            return BuildWorkspace(header, draftItems, null, supplier);
        }
        //private async Task<List<PurchaseOrderLineDto>> BuildAutoReorderLinesAsync(PurchaseOrderDirectRaiseRequestDto request)
        //{
        //    var supplierCode = request.SupplierCode.Trim().ToUpperInvariant();
        //    var locationCode = string.IsNullOrWhiteSpace(request.LocationCode)
        //        ? string.Empty
        //        : NormalizeLocation(request.LocationCode);
        //    var now = DateTime.UtcNow;
        //    var scope = request.ItemScope;

        //    var sysOptions = await _context.SysOptions.AsNoTracking().OrderByDescending(x => x.Id).FirstOrDefaultAsync();
        //    var reorderByMin = sysOptions?.POAutoReorderByMin ?? false;
        //    var reorderByMax = sysOptions?.POAutoReorderByMax ?? true;

        //    var products = await _context.ProductItems
        //        .AsNoTracking()
        //        .Where(x =>
        //            (x.Supplier1Code == supplierCode || x.Supplier2Code == supplierCode) &&
        //            x.DoNotReOrder != true &&
        //            x.IsDiscontinued != true &&
        //            (!x.Current.HasValue || x.Current != false) &&
        //            (
        //                scope == PurchaseOrderItemScope.All ||
        //                (scope == PurchaseOrderItemScope.MajorOnly && x.Major) ||
        //                (scope == PurchaseOrderItemScope.MinorOnly && !x.Major)
        //            ))
        //        .OrderBy(x => x.PartNumber)
        //        .ToListAsync();

        //    var lines = new List<PurchaseOrderLineDto>();

        //    foreach (var product in products)
        //    {
        //        var stockQty = await GetStockForLocationAsync(product.PartNumber, locationCode);
        //        var productLevel = await _context.ProductLevels.AsNoTracking().FirstOrDefaultAsync(x => x.PartNumber == product.PartNumber);

        //        var (minStock, maxStock) = string.IsNullOrWhiteSpace(locationCode)
        //            ? GetAggregateMinMax(productLevel)
        //            : GetMinMaxForLocation(productLevel, locationCode);

        //        if (minStock == 0 && maxStock == 0 && !string.IsNullOrWhiteSpace(locationCode))
        //            (minStock, maxStock) = GetAggregateMinMax(productLevel);

        //        var targetQty = reorderByMin && !reorderByMax ? minStock : maxStock;
        //        var qtyRequired = targetQty - stockQty;

        //        if (qtyRequired <= 0)
        //            continue;

        //        lines.Add(new PurchaseOrderLineDto
        //        {
        //            SequenceId = lines.Count + 1,
        //            PartNumber = Truncate(product.PartNumber, 5),
        //            MfrPartNumber = Truncate(product.MfrPartNumber ?? string.Empty, 25),
        //            Description = Truncate(product.ShortDescription ?? product.Details ?? product.PartNumber, 30),
        //            BoxQty = product.BoxQuantity ?? 1,
        //            QtyRequired = qtyRequired,
        //            CostPrice = product.CostPrice ?? 0m,
        //            QtyRecieved = 0,
        //            StockLocationCode = string.IsNullOrWhiteSpace(locationCode) ? "01" : locationCode,
        //            DeliveryLocationCode = string.IsNullOrWhiteSpace(locationCode) ? "01" : locationCode,
        //            CustomerAccNo = string.Empty,
        //            IsMajor = product.Major,
        //            XmasClub = false,
        //            SmsCustomerOnArrival = false,
        //            Notes = string.Empty,
        //            OrderedByCode = request.RaisedByStaffCode.Trim().ToUpperInvariant(),
        //            SupplierCode = supplierCode,
        //            CreatedOnDate = now,
        //            Reason = request.SalesCode?.Trim() ?? string.Empty
        //        });
        //    }

        //    return lines;
        //}

        private async Task<int> GetStockForLocationAsync(string partNumber, string locationCode)
        {
            var stockRow = await _context._ProductsStockLevels
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PartNumber == partNumber);

            if (stockRow == null)
                return 0;

            if (string.IsNullOrWhiteSpace(locationCode))
                return GetTotalStock(stockRow);

            var prop = typeof(StockLevels).GetProperty($"L{locationCode}");
            if (prop == null)
                return GetTotalStock(stockRow);

            return (int)(prop.GetValue(stockRow) ?? 0);
        }

        private static int GetTotalStock(StockLevels row)
        {
            var total = 0;
            for (var i = 1; i <= 30; i++)
            {
                var prop = typeof(StockLevels).GetProperty($"L{i:D2}");
                total += (int)(prop?.GetValue(row) ?? 0);
            }

            return total;
        }

        private static (int Min, int Max) GetMinMaxForLocation(ProductLevel? level, string locationCode)
        {
            if (level == null || string.IsNullOrWhiteSpace(locationCode) || !int.TryParse(locationCode, out var index))
                return (0, 0);

            var minProp = typeof(ProductLevel).GetProperty($"Min{index:D2}");
            var maxProp = typeof(ProductLevel).GetProperty($"Max{index:D2}");

            var min = minProp?.GetValue(level) as int? ?? 0;
            var max = maxProp?.GetValue(level) as int? ?? 0;

            return (min, max);
        }

        private static (int Min, int Max) GetAggregateMinMax(ProductLevel? level)
        {
            if (level == null)
                return (0, 0);

            var min = 0;
            var max = 0;

            for (var i = 1; i <= 30; i++)
            {
                var minProp = typeof(ProductLevel).GetProperty($"Min{i:D2}");
                var maxProp = typeof(ProductLevel).GetProperty($"Max{i:D2}");
                min += (int)(minProp?.GetValue(level) ?? 0);
                max += (int)(maxProp?.GetValue(level) ?? 0);
            }

            return (min, max);
        }

        private async Task<List<PurchaseOrderItem>> BuildLinesAsync(
            List<PurchaseOrderLineDto> lines,
            string supplierCode,
            int draftRef,
            string raisedByStaffCode)
        {
            var result = new List<PurchaseOrderItem>();
            var now = DateTime.UtcNow;
            var sequence = 1;

            foreach (var line in lines.Where(x => !string.IsNullOrWhiteSpace(x.PartNumber)))
            {
                var partNumber = line.PartNumber.Trim().ToUpperInvariant();
                var product = await _productRepository.GetByPartNumberAsync(partNumber);

                result.Add(new PurchaseOrderItem
                {
                    OrderNumber = DraftOrderNumber,
                    SequenceId = line.SequenceId > 0 ? line.SequenceId : sequence++,
                    PartNumber = Truncate(partNumber, 5),
                    MfrPartNumber = Truncate((line.MfrPartNumber ?? product?.MfrPartNumber ?? string.Empty).Trim(), 25),
                    Description = Truncate((line.Description ?? product?.ShortDescription ?? product?.Details ?? partNumber).Trim(), 30),
                    BoxQty = line.BoxQty > 0 ? line.BoxQty : (product?.BoxQuantity ?? 1),
                    QtyRequired = line.QtyRequired > 0 ? line.QtyRequired : 1,
                    CostPrice = line.CostPrice > 0 ? line.CostPrice : (product?.CostPrice ?? 0m),
                    QtyRecieved = Math.Max(line.QtyRecieved, 0),
                    StockLocationCode = Truncate(NormalizeLocation(line.StockLocationCode), 2),
                    DeliveryLocationCode = Truncate(NormalizeLocation(string.IsNullOrWhiteSpace(line.DeliveryLocationCode) ? line.StockLocationCode : line.DeliveryLocationCode), 2),
                    CustomerAccNo = Truncate(string.IsNullOrWhiteSpace(line.CustomerAccNo) ? string.Empty : line.CustomerAccNo.Trim().ToUpperInvariant(), 6),
                    IsMajor = line.IsMajor || (product?.Major ?? false),
                    XmasClub = line.XmasClub,
                    SmsCustomerOnArrival = line.SmsCustomerOnArrival,
                    Notes = Truncate(line.Notes?.Trim() ?? string.Empty, 100),
                    OrderedByCode = Truncate(string.IsNullOrWhiteSpace(line.OrderedByCode) ? raisedByStaffCode.Trim().ToUpperInvariant() : line.OrderedByCode.Trim().ToUpperInvariant(), 2),
                    SupplierCode = Truncate(supplierCode, 6),
                    CreatedOnDate = line.CreatedOnDate == default ? now : line.CreatedOnDate,
                    Reason = line.Reason?.Trim() ?? string.Empty,
                    DateCreated = now,
                    InternalOrderRefID = draftRef
                });
            }

            return result;
        }

        private async Task<int> GetNextDraftRefAsync()
        {
            var last = await _context._PurchaseOrderItems
                .AsNoTracking()
                .OrderByDescending(x => x.InternalOrderRefID)
                .Select(x => (int?)x.InternalOrderRefID)
                .FirstOrDefaultAsync();

            return (last ?? 0) + 1;
        }

        private async Task<string> GenerateNextOrderNumberAsync()
        {
            var last = await _context._PurchaseOrderHeaders
                .AsNoTracking()
                .OrderByDescending(x => x.OrderNumber)
                .Select(x => x.OrderNumber)
                .FirstOrDefaultAsync();

            return SequenceHelper.GenerateNext(last, 10);
        }

        private static string NormalizeOrderNumber(string? orderNumber)
        {
            var value = orderNumber?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Order number is required", nameof(orderNumber));

            return value.PadLeft(10, '0');
        }

        private static string NormalizeLocation(string? location)
        {
            var value = location?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(value))
                return "01";

            if (value.Length == 1)
                return $"0{value}";

            return value.Length > 2 ? value[^2..] : value;
        }

        private static string Truncate(string? value, int maxLength)
        {
            var text = value?.Trim() ?? string.Empty;
            if (maxLength <= 0 || text.Length <= maxLength)
                return text;

            return text[..maxLength];
        }

        private static string StatusText(int status)
        {
            return status switch
            {
                (int)PurchaseOrderStatus.Draft => "Draft",
                (int)PurchaseOrderStatus.Raised => "Raised",
                (int)PurchaseOrderStatus.PartReceived => "Part Received",
                (int)PurchaseOrderStatus.Received => "Received",
                (int)PurchaseOrderStatus.Closed => "Closed",
                (int)PurchaseOrderStatus.Cancelled => "Cancelled",
                _ => $"Status {status}"
            };
        }

        private PurchaseOrderWorkspaceDto BuildWorkspace(
            PurchaseOrderHeader? header,
            IEnumerable<PurchaseOrderItem> items,
            int? draftRef,
            Supplier? supplier)
        {
            var itemDtos = items
                .OrderBy(x => x.SequenceId)
                .Select(ToLineDto)
                .ToList();

            var headerDto = header == null ? null : new PurchaseOrderHeaderDto
            {
                Id = header.Id,
                OrderNumber = header.OrderNumber,
                RaisedByStaffCode = header.RaisedByStaffCode,
                RaisedOnDate = header.RaisedOnDate,
                CarriageCost = (decimal)header.CarriageCost,
                AmendedLastByCode = header.AmendedLastByCode,
                AmendedLastOnDate = header.AmendedLastOnDate,
                ClosedByCode = header.ClosedByCode,
                ClosedOnDate = header.ClosedOnDate,
                CancelledByCode = header.CancelledByCode,
                CancelledOnDate = header.CancelledOnDate,
                Status = header.Status,
                SupplierCode = header.SupplierCode,
                IsImported = header.IsImported,
                JsonReport = header.JsonReport,
                DateCreated = header.DateCreated,
                DateUpdated = header.DateUpdated,
                DirectToStore = header.DirectToStore
            };

            return new PurchaseOrderWorkspaceDto
            {
                DraftRef = draftRef ?? itemDtos.FirstOrDefault()?.InternalOrderRefID ?? 0,
                Header = headerDto,
                SupplierName = supplier?.Name ?? string.Empty,
                SupplierAddress = BuildSupplierAddress(supplier),
                JsonReport = header?.JsonReport,
                Lines = itemDtos
            };
        }

        private static PurchaseOrderLineDto ToLineDto(PurchaseOrderItem item) =>
            new()
            {
                Id = item.Id,
                OrderNumber = item.OrderNumber,
                SequenceId = item.SequenceId,
                PartNumber = item.PartNumber,
                MfrPartNumber = item.MfrPartNumber,
                Description = item.Description,
                BoxQty = item.BoxQty,
                QtyRequired = item.QtyRequired,
                CostPrice = item.CostPrice,
                QtyRecieved = item.QtyRecieved,
                StockLocationCode = item.StockLocationCode,
                DeliveryLocationCode = item.DeliveryLocationCode,
                CustomerAccNo = item.CustomerAccNo,
                IsMajor = item.IsMajor,
                XmasClub = item.XmasClub,
                SmsCustomerOnArrival = item.SmsCustomerOnArrival,
                Notes = item.Notes,
                OrderedByCode = item.OrderedByCode,
                SupplierCode = item.SupplierCode,
                CreatedOnDate = item.CreatedOnDate,
                Reason = item.Reason,
                InternalOrderRefID = item.InternalOrderRefID
            };

        private static string BuildSupplierAddress(Supplier? supplier)
        {
            if (supplier == null)
                return string.Empty;

            var parts = new[]
            {
                supplier.Name,
                supplier.Address1,
                supplier.Address2,
                supplier.Address3,
                supplier.Address4,
                supplier.Postcode
            };

            return string.Join(Environment.NewLine, parts.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        private string BuildJsonReport(
            PurchaseOrderHeader header,
            IEnumerable<PurchaseOrderItem> items,
            Supplier? supplier,
            string? locationCode,
            string footerMessage)
        {
            var envelope = new PurchaseOrderReportEnvelopeDto
            {
                Result = new PurchaseOrderReportResultDto
                {
                    ReportHeader = new PurchaseOrderReportHeaderDto
                    {
                        AccountNumber = header.SupplierCode,
                        OrderNumber = header.OrderNumber,
                        OrderDate = header.RaisedOnDate.ToString("dd/MM/yyyy"),
                        OrderDateObj = header.RaisedOnDate,
                        Status = header.Status,
                        SupplierName = supplier?.Name ?? string.Empty,
                        SupplierAccNo = supplier?.AccountNo ?? header.SupplierCode,
                        AbacusSupplierNo = supplier?.AccountNo ?? header.SupplierCode,
                        SupplierAddress = BuildSupplierAddress(supplier),
                        SupplierEmail = supplier?.Email ?? string.Empty,
                        Telephone = supplier?.Telephone ?? string.Empty,
                        Fax = supplier?.Fax ?? string.Empty,
                        FooterMessage = footerMessage ?? string.Empty,
                        CompanyAddress = string.Empty,
                        DeliveryAddress = string.Empty,
                        LocationNo = locationCode ?? string.Empty,
                        OurVatNumber = string.Empty,
                        WebAddress = string.Empty
                    },
                    Rows = items
                        .OrderBy(x => x.SequenceId)
                        .Select(x => new PurchaseOrderReportRowDto
                        {
                            YourPartNumber = x.MfrPartNumber,
                            OurPartNumber = x.PartNumber,
                            Description = x.Description,
                            Multiplier = Math.Max(x.QtyRequired, 0),
                            BoxQty = x.BoxQty,
                            QuantityOrdered = x.QtyRequired,
                            UnitCost = x.CostPrice,
                            TotalCost = x.QtyRequired * x.CostPrice,
                            CustomerAcc = x.CustomerAccNo,
                            POType = "PO",
                            IsCancelled = false
                        })
                        .ToList()
                }
            };

            return JsonSerializer.Serialize(envelope);
        }
    }
}

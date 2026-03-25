using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    public class ComboService : IComboService
    {
        private readonly IComboRepository _comboRepository;
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan ComboCacheDuration = TimeSpan.FromMinutes(20);

        public ComboService(IComboRepository comboRepository, IMemoryCache cache)
        {
            _comboRepository = comboRepository;
            _cache = cache;
        }

        public async Task<ApiResponse<ComboMasterDto>> CreateCombo(ComboSaveRequestDto request)
        {
            try
            {
                var normalized = NormalizeRequest(request);
                var validation = await BuildValidatedRequestAsync(normalized, null);
                if (!validation.Success || validation.Data == null)
                    return ApiResponse<ComboMasterDto>.Fail(validation.Message);

                var comboPartNumber = $"CO{await _comboRepository.GenerateNextComboPartNumberAsync()}";
                var entity = new ComboMaster
                {
                    ComboPartNumber = comboPartNumber,
                    ComboName = validation.Data.ComboName,
                    NumberOfProductsIncluded = validation.Data.Details.Count,
                    TotalQty = validation.Data.TotalQty,
                    TotalPrice = validation.Data.TotalPrice,
                    OfferPrice = validation.Data.OfferPrice,
                    ComboPrice = validation.Data.ComboPrice,
                    DiscountPrice = validation.Data.DiscountPrice,
                    IsActive = true,
                    CreatedOn = DateTime.Now,
                    Details = validation.Data.Details
                };

                await _comboRepository.AddAsync(entity);
                await _comboRepository.SaveChangesAsync();
                InvalidateCache(entity.ComboId, entity.ComboPartNumber);

                return ApiResponse<ComboMasterDto>.Ok(Map(entity), "Combo created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ComboMasterDto>.Fail($"Combo create failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ComboMasterDto>> UpdateCombo(ComboSaveRequestDto request)
        {
            if (!request.ComboId.HasValue || request.ComboId.Value <= 0)
                return ApiResponse<ComboMasterDto>.Fail("ComboId is required");

            try
            {
                var entity = await _comboRepository.GetByIdWithDetailsAsync(request.ComboId.Value);
                if (entity == null)
                    return ApiResponse<ComboMasterDto>.Fail("Combo not found");

                var normalized = NormalizeRequest(request);
                normalized.ComboPartNumber = entity.ComboPartNumber;

                var validation = await BuildValidatedRequestAsync(normalized, entity.ComboId);
                if (!validation.Success || validation.Data == null)
                    return ApiResponse<ComboMasterDto>.Fail(validation.Message);

                entity.ComboName = validation.Data.ComboName;
                entity.NumberOfProductsIncluded = validation.Data.Details.Count;
                entity.TotalQty = validation.Data.TotalQty;
                entity.TotalPrice = validation.Data.TotalPrice;
                entity.OfferPrice = validation.Data.OfferPrice;
                entity.ComboPrice = validation.Data.ComboPrice;
                entity.DiscountPrice = validation.Data.DiscountPrice;
                entity.UpdatedOn = DateTime.Now;

                var remainingDetails = entity.Details.ToDictionary(x => x.ComboDetailId);
                foreach (var detail in validation.Data.Details)
                {
                    ComboDetail? existing = null;
                    if (detail.ComboDetailId > 0 && remainingDetails.TryGetValue(detail.ComboDetailId, out var found))
                    {
                        existing = found;
                        remainingDetails.Remove(detail.ComboDetailId);
                    }
                    else
                    {
                        existing = entity.Details.FirstOrDefault(x => x.PartNumber == detail.PartNumber);
                        if (existing != null)
                            remainingDetails.Remove(existing.ComboDetailId);
                    }

                    if (existing == null)
                    {
                        entity.Details.Add(detail);
                        continue;
                    }

                    existing.PartNumber = detail.PartNumber;
                    existing.ProductName = detail.ProductName;
                    existing.ImageMain = detail.ImageMain;
                    existing.Qty = detail.Qty;
                    existing.UnitPrice = detail.UnitPrice;
                    existing.PromoPrice = detail.PromoPrice;
                    existing.LineTotal = detail.LineTotal;
                    existing.PromoLineTotal = detail.PromoLineTotal;
                }

                foreach (var toRemove in remainingDetails.Values.ToList())
                {
                    await _comboRepository.RemoveDetailAsync(toRemove);
                }

                await _comboRepository.SaveChangesAsync();
                InvalidateCache(entity.ComboId, entity.ComboPartNumber);

                return ApiResponse<ComboMasterDto>.Ok(Map(entity), "Combo updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ComboMasterDto>.Fail($"Combo update failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ComboGridDto>>> GetComboGrid()
            => ApiResponse<List<ComboGridDto>>.Ok(await _comboRepository.GetGridAsync());

        public async Task<ApiResponse<ComboMasterDto>> GetComboById(int comboId)
        {
            var cacheKey = GetComboIdCacheKey(comboId);
            if (_cache.TryGetValue(cacheKey, out ComboMasterDto? cached) && cached != null)
                return ApiResponse<ComboMasterDto>.Ok(cached);

            var combo = await _comboRepository.GetByIdWithDetailsAsync(comboId);
            if (combo == null)
                return ApiResponse<ComboMasterDto>.Fail("Combo not found");

            var dto = Map(combo);
            _cache.Set(cacheKey, dto, ComboCacheDuration);
            _cache.Set(GetComboPartCacheKey(combo.ComboPartNumber), dto, ComboCacheDuration);
            return ApiResponse<ComboMasterDto>.Ok(dto);
        }

        public async Task<ApiResponse<bool>> SoftDeleteCombo(int comboId)
        {
            var combo = await _comboRepository.GetByIdAsync(comboId);
            if (combo == null)
                return ApiResponse<bool>.Fail("Combo not found");

            combo.IsActive = false;
            combo.UpdatedOn = DateTime.Now;
            await _comboRepository.SaveChangesAsync();
            InvalidateCache(combo.ComboId, combo.ComboPartNumber);
            return ApiResponse<bool>.Ok(true, "Combo deleted successfully");
        }

        public async Task<ApiResponse<ComboMasterDto>> GetActiveComboByPartNumber(string comboPartNumber)
        {
            if (string.IsNullOrWhiteSpace(comboPartNumber))
                return ApiResponse<ComboMasterDto>.Fail("Combo PartNumber is required");

            comboPartNumber = comboPartNumber.Trim().ToUpperInvariant();
            var cacheKey = GetComboPartCacheKey(comboPartNumber);
            if (_cache.TryGetValue(cacheKey, out ComboMasterDto? cached) && cached != null)
                return ApiResponse<ComboMasterDto>.Ok(cached);

            var combo = await _comboRepository.GetActiveByPartNumberAsync(comboPartNumber);
            if (combo == null)
                return ApiResponse<ComboMasterDto>.Fail("Combo not found");

            var dto = Map(combo);
            _cache.Set(cacheKey, dto, ComboCacheDuration);
            _cache.Set(GetComboIdCacheKey(combo.ComboId), dto, ComboCacheDuration);
            return ApiResponse<ComboMasterDto>.Ok(dto);
        }

        public async Task<ApiResponse<PagedResultDto<ProductSearchDto>>> SearchProducts(string? term, int page, int pageSize)
            => ApiResponse<PagedResultDto<ProductSearchDto>>.Ok(await _comboRepository.SearchProductsAsync(term, page, pageSize));

        private async Task<ApiResponse<ValidatedComboRequest>> BuildValidatedRequestAsync(ComboSaveRequestDto request, int? comboId)
        {
            if (string.IsNullOrWhiteSpace(request.ComboName))
                return ApiResponse<ValidatedComboRequest>.Fail("Combo name is required");

            if (request.Details == null || request.Details.Count == 0)
                return ApiResponse<ValidatedComboRequest>.Fail("Combo must have at least 1 item");

            if (request.Details.Any(x => x.Qty <= 0))
                return ApiResponse<ValidatedComboRequest>.Fail("Qty must be greater than zero");

            var partNumbers = request.Details.Select(x => x.PartNumber).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var products = await _comboRepository.GetProductsByPartNumbersAsync(partNumbers);
            var productMap = products.ToDictionary(x => x.PartNumber, StringComparer.OrdinalIgnoreCase);

            var missing = partNumbers.Where(x => !productMap.ContainsKey(x)).ToList();
            if (missing.Count > 0)
                return ApiResponse<ValidatedComboRequest>.Fail($"Products not found: {string.Join(", ", missing)}");

            var details = new List<ComboDetail>();
            decimal totalPrice = 0m;
            decimal offerPrice = 0m;
            var totalQty = 0;

            foreach (var detail in request.Details)
            {
                var product = productMap[detail.PartNumber];
                var unitPrice = product.StorePrice ?? 0m;
                var effectivePromo = product.PromoPrice ?? unitPrice;
                var lineTotal = unitPrice * detail.Qty;
                var promoLineTotal = effectivePromo * detail.Qty;

                totalQty += detail.Qty;
                totalPrice += lineTotal;
                offerPrice += promoLineTotal;

                details.Add(new ComboDetail
                {
                    ComboDetailId = detail.ComboDetailId,
                    PartNumber = product.PartNumber,
                    ProductName = product.ShortDescription ?? product.Details ?? product.Search1 ?? product.PartNumber,
                    ImageMain = product.ImageMain,
                    Qty = detail.Qty,
                    UnitPrice = unitPrice,
                    PromoPrice = product.PromoPrice,
                    LineTotal = lineTotal,
                    PromoLineTotal = promoLineTotal
                });
            }

            var comboPrice = request.ComboPrice ?? offerPrice;
            if (comboPrice > totalPrice)
                return ApiResponse<ValidatedComboRequest>.Fail("ComboPrice must be less than or equal to TotalPrice");

            return ApiResponse<ValidatedComboRequest>.Ok(new ValidatedComboRequest
            {
                ComboName = request.ComboName!.Trim(),
                Details = details,
                TotalQty = totalQty,
                TotalPrice = totalPrice,
                OfferPrice = offerPrice,
                ComboPrice = comboPrice,
                DiscountPrice = totalPrice - comboPrice
            });
        }

        private static ComboSaveRequestDto NormalizeRequest(ComboSaveRequestDto request)
        {
            var mergedDetails = request.Details
                .Where(x => !string.IsNullOrWhiteSpace(x.PartNumber))
                .GroupBy(x => x.PartNumber.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    var first = g.First();
                    return new ComboDetailDto
                    {
                        ComboDetailId = g.Select(x => x.ComboDetailId).FirstOrDefault(x => x > 0),
                        ComboId = first.ComboId,
                        PartNumber = g.Key,
                        ProductName = first.ProductName,
                        ImageMain = first.ImageMain,
                        Qty = g.Sum(x => x.Qty)
                    };
                })
                .ToList();

            return new ComboSaveRequestDto
            {
                ComboId = request.ComboId,
                ComboPartNumber = request.ComboPartNumber?.Trim().ToUpperInvariant(),
                ComboName = request.ComboName?.Trim(),
                ComboPrice = request.ComboPrice,
                Details = mergedDetails
            };
        }

        private static ComboMasterDto Map(ComboMaster entity)
            => new()
            {
                ComboId = entity.ComboId,
                ComboPartNumber = entity.ComboPartNumber,
                ComboName = entity.ComboName,
                NumberOfProductsIncluded = entity.NumberOfProductsIncluded,
                TotalQty = entity.TotalQty,
                TotalPrice = entity.TotalPrice,
                OfferPrice = entity.OfferPrice,
                ComboPrice = entity.ComboPrice,
                DiscountPrice = entity.DiscountPrice,
                IsActive = entity.IsActive,
                CreatedOn = entity.CreatedOn,
                UpdatedOn = entity.UpdatedOn,
                Details = entity.Details
                    .OrderBy(x => x.ProductName)
                    .Select(x => new ComboDetailDto
                    {
                        ComboDetailId = x.ComboDetailId,
                        ComboId = x.ComboId,
                        PartNumber = x.PartNumber,
                        ProductName = x.ProductName,
                        ImageMain = x.ImageMain,
                        Qty = x.Qty,
                        UnitPrice = x.UnitPrice,
                        PromoPrice = x.PromoPrice,
                        LineTotal = x.LineTotal,
                        PromoLineTotal = x.PromoLineTotal
                    })
                    .ToList()
            };

        private void InvalidateCache(int comboId, string comboPartNumber)
        {
            _cache.Remove(GetComboIdCacheKey(comboId));
            _cache.Remove(GetComboPartCacheKey(comboPartNumber));
        }

        private static string GetComboIdCacheKey(int comboId)
            => $"combo:id:{comboId}";

        private static string GetComboPartCacheKey(string comboPartNumber)
            => $"combo:part:{comboPartNumber}";

        private sealed class ValidatedComboRequest
        {
            public string ComboName { get; set; } = "";
            public List<ComboDetail> Details { get; set; } = new();
            public int TotalQty { get; set; }
            public decimal TotalPrice { get; set; }
            public decimal OfferPrice { get; set; }
            public decimal ComboPrice { get; set; }
            public decimal DiscountPrice { get; set; }
        }
    }
}

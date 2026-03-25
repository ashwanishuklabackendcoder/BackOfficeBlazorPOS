using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class ComboRepository : IComboRepository
    {
        private readonly BackOfficeAdminContext _context;

        public ComboRepository(BackOfficeAdminContext context)
        {
            _context = context;
        }

        public async Task<int> GenerateNextComboPartNumberAsync()
        {
            var maxComboPartNumber = await _context.ComboMasters
                .AsNoTracking()
                .Select(x => x.ComboPartNumber)
                .ToListAsync();

            var numericMax = maxComboPartNumber
                .Select(x =>
                {
                    var normalized = string.IsNullOrWhiteSpace(x) ? "" : x.Trim();
                    if (normalized.StartsWith("CO", StringComparison.OrdinalIgnoreCase))
                        normalized = normalized[2..];

                    return int.TryParse(normalized, out var value) ? value : 0;
                })
                .DefaultIfEmpty(900000)
                .Max();

            return numericMax < 900000 ? 900001 : numericMax + 1;
        }

        public Task<bool> ComboPartNumberExistsAsync(string comboPartNumber, int? excludeComboId = null)
        {
            var query = _context.ComboMasters.AsNoTracking().Where(x => x.ComboPartNumber == comboPartNumber);
            if (excludeComboId.HasValue)
                query = query.Where(x => x.ComboId != excludeComboId.Value);

            return query.AnyAsync();
        }

        public Task<ComboMaster?> GetByIdAsync(int comboId)
            => _context.ComboMasters.FirstOrDefaultAsync(x => x.ComboId == comboId);

        public Task<ComboMaster?> GetByIdWithDetailsAsync(int comboId)
            => _context.ComboMasters
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.ComboId == comboId);

        public Task<ComboMaster?> GetActiveByPartNumberAsync(string comboPartNumber)
            => _context.ComboMasters
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.ComboPartNumber == comboPartNumber && x.IsActive);

        public Task<List<ComboGridDto>> GetGridAsync()
            => _context.ComboMasters
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => new ComboGridDto
                {
                    ComboId = x.ComboId,
                    ComboPartNumber = x.ComboPartNumber,
                    ComboName = x.ComboName,
                    ProductsCount = x.NumberOfProductsIncluded,
                    TotalQty = x.TotalQty,
                    TotalPrice = x.TotalPrice,
                    OfferPrice = x.OfferPrice,
                    ComboPrice = x.ComboPrice,
                    DiscountPrice = x.DiscountPrice,
                    IsActive = x.IsActive,
                    CreatedOn = x.CreatedOn,
                    UpdatedOn = x.UpdatedOn
                })
                .ToListAsync();

        public async Task<PagedResultDto<ProductSearchDto>> SearchProductsAsync(string? term, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 50);
            term = term?.Trim();

            var query = _context.ProductItems
                .AsNoTracking()
                .Where(x => !string.IsNullOrWhiteSpace(x.PartNumber));

            if (!string.IsNullOrWhiteSpace(term))
            {
                query = query.Where(x =>
                    x.PartNumber.Contains(term) ||
                    (x.ShortDescription ?? "").Contains(term) ||
                    (x.Search1 ?? "").Contains(term) ||
                    (x.Barcode ?? "").Contains(term));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.PartNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ProductSearchDto
                {
                    PartNumber = x.PartNumber,
                    ShortDescription = x.ShortDescription ?? x.Details ?? "",
                    StorePrice = x.StorePrice ?? 0m,
                    PromoPrice = x.PromoPrice,
                    ImageMain = x.ImageMain,
                    Image2 = x.Image2,
                    Image3 = x.Image3,
                    Image4 = x.Image4
                })
                .ToListAsync();

            return new PagedResultDto<ProductSearchDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            };
        }

        public Task<List<ProductItem>> GetProductsByPartNumbersAsync(List<string> partNumbers)
            => _context.ProductItems
                .Where(x => partNumbers.Contains(x.PartNumber))
                .ToListAsync();

        public Task AddAsync(ComboMaster entity)
            => _context.ComboMasters.AddAsync(entity).AsTask();

        public Task AddDetailAsync(ComboDetail entity)
            => _context.ComboDetails.AddAsync(entity).AsTask();

        public Task RemoveDetailAsync(ComboDetail entity)
        {
            _context.ComboDetails.Remove(entity);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
            => _context.SaveChangesAsync();
    }
}

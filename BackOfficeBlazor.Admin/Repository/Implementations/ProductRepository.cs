using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private readonly BackOfficeAdminContext _context;
        public ProductRepository(BackOfficeAdminContext context) => _context = context;

        public Task<ProductItem?> GetByPartNumberAsync(string partNumber)
            => _context.ProductItems.FirstOrDefaultAsync(p => p.PartNumber == partNumber);
        public Task<ProductItem?> GetByMfrPartNumberAsync(string MfrpartNumber)
        => _context.ProductItems.FirstOrDefaultAsync(p => p.MfrPartNumber == MfrpartNumber);
        public Task<ProductItem?> GetByBarcodeNumberAsync(string Barcode)
     => _context.ProductItems.FirstOrDefaultAsync(p => p.Barcode == Barcode);

        public Task<string?> GetLastPartNumberAsync()
            => _context.ProductItems
                .OrderByDescending(p => p.PartNumber)
                .Select(p => p.PartNumber)
                .FirstOrDefaultAsync();
  

        public async Task AddAsync(ProductItem entity)
        {
            await _context.ProductItems.AddAsync(entity);
        }

        public Task UpdateAsync(ProductItem entity)
        {
            _context.ProductItems.Update(entity);
            return Task.CompletedTask;
        }


        public async Task<List<ProductItem>> GetAllAsync(ProductFilterDto f)
        {
            var q = _context.ProductItems.AsNoTracking();

            bool hasFilter =
                !string.IsNullOrEmpty(f.MfrPartNumber) ||
                !string.IsNullOrEmpty(f.Supplier1Code) ||
                !string.IsNullOrEmpty(f.Search1) ||
                !string.IsNullOrEmpty(f.Search2) ||
                !string.IsNullOrEmpty(f.Size) ||
                !string.IsNullOrEmpty(f.Color) ||
                !string.IsNullOrEmpty(f.Details) ||
                !string.IsNullOrEmpty(f.Year) ||
                !string.IsNullOrEmpty(f.CatACode) ||
                !string.IsNullOrEmpty(f.CatBCode) ||
                !string.IsNullOrEmpty(f.CatCCode) ||
                !string.IsNullOrEmpty(f.Make) ||
                f.Gender.HasValue ||
                f.Website.HasValue;

            if (!hasFilter)
                return await q.OrderBy(x => x.PartNumber).ToListAsync();

            return await q
                .Where(x =>
                    (!string.IsNullOrEmpty(f.MfrPartNumber) && x.MfrPartNumber == f.MfrPartNumber) ||
                    (!string.IsNullOrEmpty(f.Supplier1Code) && x.Supplier1Code == f.Supplier1Code) ||

                    (!string.IsNullOrEmpty(f.Search1) && x.Search1.Contains(f.Search1)) ||
                    (!string.IsNullOrEmpty(f.Search2) && x.Search2.Contains(f.Search2)) ||
                    (!string.IsNullOrEmpty(f.Size) && x.Size.Contains(f.Size)) ||
                    (!string.IsNullOrEmpty(f.Color) && x.Color.Contains(f.Color)) ||
                    (!string.IsNullOrEmpty(f.Details) && x.Details.Contains(f.Details)) ||

                    (!string.IsNullOrEmpty(f.Year) && x.Year == f.Year) ||

                    (!string.IsNullOrEmpty(f.CatACode) && x.CatACode == f.CatACode) ||
                    (!string.IsNullOrEmpty(f.CatBCode) && x.CatBCode == f.CatBCode) ||
                    (!string.IsNullOrEmpty(f.CatCCode) && x.CatCCode == f.CatCCode) ||

                    (!string.IsNullOrEmpty(f.Make) && x.Make == f.Make) ||

                    (f.Gender.HasValue && x.Gender == f.Gender) ||
                    (f.Website.HasValue && x.Website == f.Website)
                )
                .OrderBy(x => x.PartNumber)
                .ToListAsync();
        }



        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}

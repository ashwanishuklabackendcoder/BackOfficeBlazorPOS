using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class ProductGroupRepository : IProductGroupRepository
    {
        private readonly BackOfficeAdminContext _db;

        public ProductGroupRepository(BackOfficeAdminContext db)
        {
            _db = db;
        }

        public async Task AddAsync(ProductGroup entity)
        {
            await _db.ProductGroups.AddAsync(entity);
        }

        public Task<ProductGroup?> GetByGroupCodeAsync(string groupCode)
            => _db.ProductGroups
                .OrderByDescending(x => x.CreatedOn)
                .FirstOrDefaultAsync(x => x.GroupCode == groupCode);

        public Task UpdateAsync(ProductGroup entity)
        {
            _db.ProductGroups.Update(entity);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
        public Task<string?> GetLastGroupNumberAsync()
         => _db.ProductGroups
             .OrderByDescending(p => p.GroupCode)
             .Select(p => p.GroupCode)
             .FirstOrDefaultAsync();
    }

}

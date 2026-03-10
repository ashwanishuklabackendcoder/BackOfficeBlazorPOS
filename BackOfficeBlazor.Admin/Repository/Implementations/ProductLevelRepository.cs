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
    public class ProductLevelRepository : IProductLevelRepository
    {
        private readonly BackOfficeAdminContext _context;
        public ProductLevelRepository(BackOfficeAdminContext context) => _context = context;

        public Task<ProductLevel?> GetByPartNumberAsync(string partNumber)
            => _context.ProductLevels.FirstOrDefaultAsync(x => x.PartNumber == partNumber);

        public async Task AddAsync(ProductLevel entity)
        {
            await _context.ProductLevels.AddAsync(entity);
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}

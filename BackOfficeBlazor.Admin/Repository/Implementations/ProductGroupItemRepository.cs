using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class ProductGroupItemRepository : IProductGroupItemRepository
    {
        private readonly BackOfficeAdminContext _db;

        public ProductGroupItemRepository(BackOfficeAdminContext db)
        {
            _db = db;
        }

        public async Task AddAsync(ProductGroupItem entity)
        {
            await _db.ProductGroupItems.AddAsync(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }

}

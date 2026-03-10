using BackOfficeBlazor.Admin.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface IProductGroupRepository
    {
        Task AddAsync(ProductGroup entity);
        Task SaveChangesAsync();
        Task<string?> GetLastGroupNumberAsync();
    }

}

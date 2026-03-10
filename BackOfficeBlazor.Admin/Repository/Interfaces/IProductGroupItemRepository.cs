using BackOfficeBlazor.Admin.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface IProductGroupItemRepository
    {
        Task AddAsync(ProductGroupItem entity);
        Task<bool> ExistsAsync(int groupId, string partNumber);
        Task SaveChangesAsync();
    }

}

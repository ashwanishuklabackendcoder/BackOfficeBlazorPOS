using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface ISupplierRepository
    {
        Task<List<Supplier>> GetAllSupplier();
        Task<Supplier?> GetByAccountNoAsync(string accNo);
        Task AddAsync(Supplier supplier);
        Task UpdateAsync(Supplier supplier);
        Task SaveChangesAsync();
        Task<string?> GetLastAccountNumberAsync();
        Task<List<SupplierDto>> SuggestSuppliersAsync(string query);
    }
}

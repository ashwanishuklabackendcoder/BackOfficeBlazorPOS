using BackOfficeBlazor.Admin.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface IManufacturerRepository
    {
        Task<List<Manufacturer>> GetAllManufacturer();

        Task<Manufacturer?> GetByCodeAsync(string code);
        Task<Manufacturer?> GetByNameAsync(string name);
        Task<string?> GetLastCodeAsync();
        Task AddAsync(Manufacturer entity);
        Task UpdateAsync(Manufacturer entity);
        Task DeleteAsync(Manufacturer entity);
        Task SaveChangesAsync();
        Task<List<string>> SuggestMakesAsync(string term);

    }
}

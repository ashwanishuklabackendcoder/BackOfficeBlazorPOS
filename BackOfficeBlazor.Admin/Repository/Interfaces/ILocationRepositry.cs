using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface IlocationRepositry
    {
        Task<List<Location>> GetAllLocation();
        Task<List<Location>> GetActiveLocations();
        Task<Location?> GetByIdAsync(int id);
        Task<Location?> GetByCodeAsync(string code);
        Task AddAsync(Location location);
        Task UpdateAsync(Location location);
        Task SaveChangesAsync();
        Task<bool> CodeExistsAsync(string code, int? excludeId);
    }
}

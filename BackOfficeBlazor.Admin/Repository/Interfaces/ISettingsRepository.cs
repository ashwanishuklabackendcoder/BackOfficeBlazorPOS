using BackOfficeBlazor.Admin.Entities;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface ISettingsRepository
    {
        Task<Setting?> GetByBranchIdAsync(int branchId);
        Task AddAsync(Setting setting);
        Task UpdateAsync(Setting setting);
        Task SaveChangesAsync();
    }
}

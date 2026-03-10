using BackOfficeBlazor.Admin.Entities;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface ISysOptionRepository
    {
        Task<SysOption?> GetAsync();
        Task AddAsync(SysOption option);
        Task UpdateAsync(SysOption option);
        Task SaveChangesAsync();
    }
}

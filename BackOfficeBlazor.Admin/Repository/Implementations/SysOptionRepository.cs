using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class SysOptionRepository : ISysOptionRepository
    {
        private readonly BackOfficeAdminContext _context;

        public SysOptionRepository(BackOfficeAdminContext context)
        {
            _context = context;
        }

        public async Task<SysOption?> GetAsync()
        {
            return await _context.SysOptions.FirstOrDefaultAsync();
        }

        public async Task AddAsync(SysOption option)
        {
            await _context.SysOptions.AddAsync(option);
        }

        public async Task UpdateAsync(SysOption option)
        {
            _context.SysOptions.Update(option);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

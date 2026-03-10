using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly BackOfficeAdminContext _context;

        public SettingsRepository(BackOfficeAdminContext context)
        {
            _context = context;
        }

        public async Task<Setting?> GetByBranchIdAsync(int branchId)
        {
            return await _context.Settings
                .FirstOrDefaultAsync(x => x.BranchId == branchId);
        }

        public async Task AddAsync(Setting setting)
        {
            await _context.Settings.AddAsync(setting);
        }

        public async Task UpdateAsync(Setting setting)
        {
            _context.Settings.Update(setting);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

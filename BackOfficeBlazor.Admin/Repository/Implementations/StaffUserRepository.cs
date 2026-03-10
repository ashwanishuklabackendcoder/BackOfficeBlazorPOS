using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class StaffUserRepository : IStaffUserRepository
    {
        private readonly BackOfficeAdminContext _context;

        public StaffUserRepository(BackOfficeAdminContext context)
        {
            _context = context;
        }

        public Task<StaffUser?> GetByIdAsync(int id)
            => _context.StaffUsers
                .Include(x => x.Permission)
                .Include(x => x.PermissionEntries)
                .FirstOrDefaultAsync(x => x.Id == id);

        public Task<StaffUser?> GetByUsernameAsync(string username)
            => _context.StaffUsers
                .Include(x => x.Permission)
                .Include(x => x.PermissionEntries)
                .FirstOrDefaultAsync(x => x.Username == username);

        public Task<List<StaffUser>> GetAllAsync()
            => _context.StaffUsers
                .Include(x => x.Permission)
                .Include(x => x.PermissionEntries)
                .OrderBy(x => x.Username)
                .ToListAsync();

        public Task AddAsync(StaffUser user)
            => _context.StaffUsers.AddAsync(user).AsTask();

        public Task SaveChangesAsync()
            => _context.SaveChangesAsync();
    }
}

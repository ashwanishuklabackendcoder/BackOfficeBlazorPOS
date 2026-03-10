using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class StaffUserPermissionRepository : IStaffUserPermissionRepository
    {
        private readonly BackOfficeAdminContext _context;

        public StaffUserPermissionRepository(BackOfficeAdminContext context)
        {
            _context = context;
        }

        public Task<StaffUserPermission?> GetByUserIdAsync(int userId)
            => _context.StaffUserPermissions.FirstOrDefaultAsync(x => x.StaffUserId == userId);

        public Task AddAsync(StaffUserPermission permission)
            => _context.StaffUserPermissions.AddAsync(permission).AsTask();

        public Task SaveChangesAsync()
            => _context.SaveChangesAsync();
    }
}

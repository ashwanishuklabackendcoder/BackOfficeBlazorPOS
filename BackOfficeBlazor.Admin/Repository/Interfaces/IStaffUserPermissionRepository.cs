using BackOfficeBlazor.Admin.Entities;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface IStaffUserPermissionRepository
    {
        Task<StaffUserPermission?> GetByUserIdAsync(int userId);
        Task AddAsync(StaffUserPermission permission);
        Task SaveChangesAsync();
    }
}

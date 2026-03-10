using BackOfficeBlazor.Admin.Entities;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface IStaffUserRepository
    {
        Task<StaffUser?> GetByIdAsync(int id);
        Task<StaffUser?> GetByUsernameAsync(string username);
        Task<List<StaffUser>> GetAllAsync();
        Task AddAsync(StaffUser user);
        Task SaveChangesAsync();
    }
}

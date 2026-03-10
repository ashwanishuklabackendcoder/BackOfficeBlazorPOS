using BackOfficeBlazor.Shared.DTOs;

namespace BackOfficeBlazor.Admin.Services.Interfaces
{
    public interface IStaffUserService
    {
        Task<ApiResponse<List<StaffUserDto>>> GetAllAsync();
        Task<ApiResponse<StaffUserDto>> CreateAsync(CreateStaffUserDto dto);
        Task<ApiResponse<StaffUserDto>> UpdateAsync(UpdateStaffUserDto dto);
        Task<ApiResponse<bool>> ResetPasswordAsync(ResetStaffUserPasswordDto dto);
        Task<ApiResponse<StaffUserPermissionDto>> SetPermissionsAsync(UpdateStaffUserPermissionsDto dto);
        Task<ApiResponse<StaffUserPermissionDto>> GetPermissionsAsync(int userId);
        Task<ApiResponse<List<string>>> SetPermissionKeysAsync(UpdateStaffUserPermissionKeysDto dto);
        Task<ApiResponse<List<string>>> GetPermissionKeysAsync(int userId);
    }
}

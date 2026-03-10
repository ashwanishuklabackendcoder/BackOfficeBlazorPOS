using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public interface IStaffUserService
    {
        Task<ApiResponse<List<StaffUserDto>>> GetAllAsync();
        Task<ApiResponse<StaffUserDto>> CreateAsync(CreateStaffUserDto dto);
        Task<ApiResponse<StaffUserDto>> UpdateAsync(UpdateStaffUserDto dto);
        Task<ApiResponse<StaffUserPermissionDto>> SavePermissionsAsync(UpdateStaffUserPermissionsDto dto);
        Task<ApiResponse<List<string>>> SavePermissionKeysAsync(UpdateStaffUserPermissionKeysDto dto);
        Task<ApiResponse<List<string>>> GetPermissionKeysAsync(int userId);
        Task<ApiResponse<bool>> ResetPasswordAsync(ResetStaffUserPasswordDto dto);
    }
}

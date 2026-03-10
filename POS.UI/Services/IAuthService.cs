using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public interface IAuthService
    {
        bool IsAuthenticated { get; }
        StaffUserDto? CurrentUser { get; }
        Task InitializeAsync();
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request);
        Task LogoutAsync();
        bool HasAccess(string permissionKey);
        bool HasModuleAccess(Func<StaffUserPermissionDto, bool> selector);
    }
}

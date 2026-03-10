using BackOfficeBlazor.Shared.DTOs;

namespace BackOfficeBlazor.Admin.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<StaffUserDto>> ValidateCredentialsAsync(LoginRequestDto request);
    }
}

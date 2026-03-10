using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Admin.Services.Security;
using BackOfficeBlazor.Shared.DTOs;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IStaffUserRepository _users;

        public AuthService(IStaffUserRepository users)
        {
            _users = users;
        }

        public async Task<ApiResponse<StaffUserDto>> ValidateCredentialsAsync(LoginRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return ApiResponse<StaffUserDto>.Fail("Username and password are required");

            var user = await _users.GetByUsernameAsync(request.Username.Trim());
            if (user == null || !user.IsActive)
                return ApiResponse<StaffUserDto>.Fail("Invalid credentials");

            var ok = PasswordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt);
            if (!ok)
                return ApiResponse<StaffUserDto>.Fail("Invalid credentials");

            return ApiResponse<StaffUserDto>.Ok(StaffUserMapper.ToDto(user));
        }
    }
}

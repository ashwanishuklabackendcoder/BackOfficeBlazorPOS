using BackOfficeBlazor.Shared.DTOs;

namespace POSAPI.Services
{
    public interface IJwtTokenService
    {
        LoginResponseDto CreateToken(StaffUserDto user);
    }
}

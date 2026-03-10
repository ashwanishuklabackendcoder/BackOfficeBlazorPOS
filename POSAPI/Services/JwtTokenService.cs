using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using POSAPI.Config;

namespace POSAPI.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _settings;

        public JwtTokenService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        public LoginResponseDto CreateToken(StaffUserDto user)
        {
            var expires = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Username),
                new("user_id", user.Id.ToString()),
                new("full_name", user.FullName),
                new("staff_code", user.StaffCode),
                new("is_admin", user.IsAdmin.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new LoginResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAtUtc = expires,
                User = user
            };
        }
    }
}

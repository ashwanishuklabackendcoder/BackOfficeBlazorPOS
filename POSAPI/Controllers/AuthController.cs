using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSAPI.Services;

namespace POSAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(IAuthService authService, IJwtTokenService jwtTokenService)
        {
            _authService = authService;
            _jwtTokenService = jwtTokenService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.ValidateCredentialsAsync(request);
            if (!result.Success || result.Data == null)
                return Unauthorized(ApiResponse<object>.Fail(result.Message));

            var token = _jwtTokenService.CreateToken(result.Data);
            return Ok(ApiResponse<LoginResponseDto>.Ok(token, "Login successful"));
        }
    }
}

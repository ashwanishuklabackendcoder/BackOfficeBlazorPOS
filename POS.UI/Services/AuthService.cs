using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.JSInterop;

namespace POS.UI.Services
{
    public class AuthService : IAuthService
    {
        private const string TokenKey = "pos_auth_token";
        private const string UserKey = "pos_auth_user";

        private readonly HttpClient _http;
        private readonly IJSRuntime _js;

        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(_token) && CurrentUser != null;
        public StaffUserDto? CurrentUser { get; private set; }

        private string? _token;

        public AuthService(HttpClient http, IJSRuntime js)
        {
            _http = http;
            _js = js;
        }

        public async Task InitializeAsync()
        {
            _token = await _js.InvokeAsync<string>("localStorage.getItem", TokenKey);
            var userJson = await _js.InvokeAsync<string>("localStorage.getItem", UserKey);

            if (!string.IsNullOrWhiteSpace(userJson))
            {
                CurrentUser = JsonSerializer.Deserialize<StaffUserDto>(
                    userJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            ApplyTokenHeader(_token);
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", request);
            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>()
                          ?? ApiResponse<LoginResponseDto>.Fail("Invalid server response");

            if (!response.IsSuccessStatusCode || !payload.Success || payload.Data == null)
                return ApiResponse<LoginResponseDto>.Fail(payload.Message);

            _token = payload.Data.Token;
            CurrentUser = payload.Data.User;

            await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, _token);
            await _js.InvokeVoidAsync("localStorage.setItem", UserKey, JsonSerializer.Serialize(CurrentUser));
            ApplyTokenHeader(_token);

            return payload;
        }

        public async Task LogoutAsync()
        {
            _token = null;
            CurrentUser = null;
            ApplyTokenHeader(null);
            await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
            await _js.InvokeVoidAsync("localStorage.removeItem", UserKey);
        }

        public bool HasModuleAccess(Func<StaffUserPermissionDto, bool> selector)
        {
            if (CurrentUser == null)
                return false;

            if (CurrentUser.IsAdmin)
                return true;

            return selector(CurrentUser.Permissions ?? new StaffUserPermissionDto());
        }

        public bool HasAccess(string permissionKey)
        {
            if (CurrentUser == null)
                return false;

            if (CurrentUser.IsAdmin)
                return true;

            var keys = CurrentUser.PermissionKeys ?? new List<string>();
            if (keys.Any(x => string.Equals(x, permissionKey, StringComparison.OrdinalIgnoreCase)))
                return true;

            // Backward compatibility for users that only have legacy bool permissions.
            var legacyKeys = PermissionCatalog.ToKeys(CurrentUser.Permissions ?? new StaffUserPermissionDto());
            return legacyKeys.Any(x => string.Equals(x, permissionKey, StringComparison.OrdinalIgnoreCase));
        }

        private void ApplyTokenHeader(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _http.DefaultRequestHeaders.Authorization = null;
                return;
            }

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
}

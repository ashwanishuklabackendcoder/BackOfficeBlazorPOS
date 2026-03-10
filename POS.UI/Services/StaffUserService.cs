using System.Net.Http.Json;
using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public class StaffUserService : IStaffUserService
    {
        private readonly HttpClient _http;

        public StaffUserService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ApiResponse<List<StaffUserDto>>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<ApiResponse<List<StaffUserDto>>>("api/staff-users")
                   ?? ApiResponse<List<StaffUserDto>>.Fail("Null response");
        }

        public async Task<ApiResponse<StaffUserDto>> CreateAsync(CreateStaffUserDto dto)
        {
            var res = await _http.PostAsJsonAsync("api/staff-users", dto);
            return await res.Content.ReadFromJsonAsync<ApiResponse<StaffUserDto>>()
                   ?? ApiResponse<StaffUserDto>.Fail("Null response");
        }

        public async Task<ApiResponse<StaffUserDto>> UpdateAsync(UpdateStaffUserDto dto)
        {
            var res = await _http.PutAsJsonAsync("api/staff-users", dto);
            return await res.Content.ReadFromJsonAsync<ApiResponse<StaffUserDto>>()
                   ?? ApiResponse<StaffUserDto>.Fail("Null response");
        }

        public async Task<ApiResponse<StaffUserPermissionDto>> SavePermissionsAsync(UpdateStaffUserPermissionsDto dto)
        {
            var res = await _http.PutAsJsonAsync("api/staff-users/permissions", dto);
            return await res.Content.ReadFromJsonAsync<ApiResponse<StaffUserPermissionDto>>()
                   ?? ApiResponse<StaffUserPermissionDto>.Fail("Null response");
        }

        public async Task<ApiResponse<List<string>>> SavePermissionKeysAsync(UpdateStaffUserPermissionKeysDto dto)
        {
            var res = await _http.PutAsJsonAsync("api/staff-users/permission-keys", dto);
            return await res.Content.ReadFromJsonAsync<ApiResponse<List<string>>>()
                   ?? ApiResponse<List<string>>.Fail("Null response");
        }

        public async Task<ApiResponse<List<string>>> GetPermissionKeysAsync(int userId)
        {
            return await _http.GetFromJsonAsync<ApiResponse<List<string>>>($"api/staff-users/{userId}/permission-keys")
                   ?? ApiResponse<List<string>>.Fail("Null response");
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetStaffUserPasswordDto dto)
        {
            var res = await _http.PutAsJsonAsync("api/staff-users/reset-password", dto);
            return await res.Content.ReadFromJsonAsync<ApiResponse<bool>>()
                   ?? ApiResponse<bool>.Fail("Null response");
        }
    }
}

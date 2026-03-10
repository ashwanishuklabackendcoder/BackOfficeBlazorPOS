using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Admin.Services.Security;
using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    public class StaffUserService : IStaffUserService
    {
        private readonly IStaffUserRepository _users;
        private readonly IStaffUserPermissionRepository _permissions;
        private readonly BackOfficeAdminContext _db;

        public StaffUserService(
            IStaffUserRepository users,
            IStaffUserPermissionRepository permissions,
            BackOfficeAdminContext db)
        {
            _users = users;
            _permissions = permissions;
            _db = db;
        }

        public async Task<ApiResponse<List<StaffUserDto>>> GetAllAsync()
        {
            var users = await _users.GetAllAsync();
            return ApiResponse<List<StaffUserDto>>.Ok(users.Select(StaffUserMapper.ToDto).ToList());
        }

        public async Task<ApiResponse<StaffUserDto>> CreateAsync(CreateStaffUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return ApiResponse<StaffUserDto>.Fail("Username and password are required");

            var exists = await _users.GetByUsernameAsync(dto.Username.Trim());
            if (exists != null)
                return ApiResponse<StaffUserDto>.Fail("Username already exists");

            var (hash, salt) = PasswordHasher.HashPassword(dto.Password);

            var user = new StaffUser
            {
                Username = dto.Username.Trim(),
                FullName = dto.FullName.Trim(),
                StaffCode = dto.StaffCode.Trim(),
                PasswordHash = hash,
                PasswordSalt = salt,
                IsAdmin = dto.IsAdmin,
                IsActive = dto.IsActive,
                DateCreated = DateTime.UtcNow
            };

            await _users.AddAsync(user);
            await _users.SaveChangesAsync();

            var perm = new StaffUserPermission { StaffUserId = user.Id };
            await _permissions.AddAsync(perm);
            await _permissions.SaveChangesAsync();

            user.Permission = perm;
            return ApiResponse<StaffUserDto>.Ok(StaffUserMapper.ToDto(user), "User created");
        }

        public async Task<ApiResponse<StaffUserDto>> UpdateAsync(UpdateStaffUserDto dto)
        {
            var user = await _users.GetByIdAsync(dto.Id);
            if (user == null)
                return ApiResponse<StaffUserDto>.Fail("User not found");

            user.FullName = dto.FullName.Trim();
            user.StaffCode = dto.StaffCode.Trim();
            user.IsAdmin = dto.IsAdmin;
            user.IsActive = dto.IsActive;
            user.DateUpdated = DateTime.UtcNow;

            await _users.SaveChangesAsync();
            return ApiResponse<StaffUserDto>.Ok(StaffUserMapper.ToDto(user), "User updated");
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetStaffUserPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                return ApiResponse<bool>.Fail("New password is required");

            var user = await _users.GetByIdAsync(dto.UserId);
            if (user == null)
                return ApiResponse<bool>.Fail("User not found");

            var (hash, salt) = PasswordHasher.HashPassword(dto.NewPassword);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.DateUpdated = DateTime.UtcNow;
            await _users.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Password updated");
        }

        public async Task<ApiResponse<StaffUserPermissionDto>> SetPermissionsAsync(UpdateStaffUserPermissionsDto dto)
        {
            var user = await _users.GetByIdAsync(dto.UserId);
            if (user == null)
                return ApiResponse<StaffUserPermissionDto>.Fail("User not found");

            var permission = await _permissions.GetByUserIdAsync(dto.UserId);
            if (permission == null)
            {
                permission = new StaffUserPermission { StaffUserId = dto.UserId };
                await _permissions.AddAsync(permission);
            }

            StaffUserMapper.ApplyPermissions(permission, dto.Permissions);
            await ReplacePermissionEntriesAsync(dto.UserId, PermissionCatalog.ToKeys(dto.Permissions));
            await _permissions.SaveChangesAsync();

            return ApiResponse<StaffUserPermissionDto>.Ok(
                StaffUserMapper.ToPermissionDto(permission),
                "Permissions updated");
        }

        public async Task<ApiResponse<StaffUserPermissionDto>> GetPermissionsAsync(int userId)
        {
            var user = await _users.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse<StaffUserPermissionDto>.Fail("User not found");

            return ApiResponse<StaffUserPermissionDto>.Ok(
                StaffUserMapper.ToPermissionDto(user.Permission));
        }

        public async Task<ApiResponse<List<string>>> SetPermissionKeysAsync(UpdateStaffUserPermissionKeysDto dto)
        {
            var user = await _users.GetByIdAsync(dto.UserId);
            if (user == null)
                return ApiResponse<List<string>>.Fail("User not found");

            var normalized = StaffUserMapper.NormalizeKeys(dto.PermissionKeys);
            await ReplacePermissionEntriesAsync(dto.UserId, normalized);

            var permission = await _permissions.GetByUserIdAsync(dto.UserId);
            if (permission == null)
            {
                permission = new StaffUserPermission { StaffUserId = dto.UserId };
                await _permissions.AddAsync(permission);
            }

            StaffUserMapper.ApplyPermissions(permission, PermissionCatalog.ToLegacyDto(normalized));
            await _permissions.SaveChangesAsync();

            return ApiResponse<List<string>>.Ok(normalized, "Permission keys updated");
        }

        public async Task<ApiResponse<List<string>>> GetPermissionKeysAsync(int userId)
        {
            var user = await _users.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse<List<string>>.Fail("User not found");

            var keys = user.PermissionEntries
                .Select(x => x.PermissionKey)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (keys.Count == 0)
                keys = PermissionCatalog.ToKeys(StaffUserMapper.ToPermissionDto(user.Permission));

            return ApiResponse<List<string>>.Ok(keys);
        }

        private async Task ReplacePermissionEntriesAsync(int userId, List<string> keys)
        {
            var existing = await _db.StaffUserPermissionEntries
                .Where(x => x.StaffUserId == userId)
                .ToListAsync();

            if (existing.Count > 0)
                _db.StaffUserPermissionEntries.RemoveRange(existing);

            if (keys.Count > 0)
            {
                var rows = keys.Select(k => new StaffUserPermissionEntry
                {
                    StaffUserId = userId,
                    PermissionKey = k
                });
                await _db.StaffUserPermissionEntries.AddRangeAsync(rows);
            }
        }
    }
}

namespace BackOfficeBlazor.Shared.DTOs
{
    public class LoginRequestDto
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = "";
        public DateTime ExpiresAtUtc { get; set; }
        public StaffUserDto User { get; set; } = new();
    }

    public class CreateStaffUserDto
    {
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string StaffCode { get; set; } = "";
        public string Password { get; set; } = "";
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateStaffUserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string StaffCode { get; set; } = "";
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
    }

    public class ResetStaffUserPasswordDto
    {
        public int UserId { get; set; }
        public string NewPassword { get; set; } = "";
    }

    public class UpdateStaffUserPermissionsDto
    {
        public int UserId { get; set; }
        public StaffUserPermissionDto Permissions { get; set; } = new();
    }

    public class UpdateStaffUserPermissionKeysDto
    {
        public int UserId { get; set; }
        public List<string> PermissionKeys { get; set; } = new();
    }
}

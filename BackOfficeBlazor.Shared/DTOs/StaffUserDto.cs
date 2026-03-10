using System.ComponentModel.DataAnnotations;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class StaffUserDto
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string Username { get; set; } = "";

        [StringLength(100)]
        public string FullName { get; set; } = "";

        [StringLength(10)]
        public string StaffCode { get; set; } = "";
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public StaffUserPermissionDto Permissions { get; set; } = new();
        public List<string> PermissionKeys { get; set; } = new();
    }
}

using System.ComponentModel.DataAnnotations;

namespace BackOfficeBlazor.Admin.Entities
{
    public class StaffUser
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; } = "";

        [Required, StringLength(100)]
        public string FullName { get; set; } = "";

        [Required, StringLength(10)]
        public string StaffCode { get; set; } = "";

        [Required]
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

        [Required]
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? DateUpdated { get; set; }

        public StaffUserPermission? Permission { get; set; }
        public ICollection<StaffUserPermissionEntry> PermissionEntries { get; set; } = new List<StaffUserPermissionEntry>();
    }
}

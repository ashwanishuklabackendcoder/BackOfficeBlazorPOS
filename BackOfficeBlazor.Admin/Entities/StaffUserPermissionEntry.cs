using System.ComponentModel.DataAnnotations;

namespace BackOfficeBlazor.Admin.Entities
{
    public class StaffUserPermissionEntry
    {
        [Key]
        public int Id { get; set; }

        public int StaffUserId { get; set; }

        [Required, StringLength(120)]
        public string PermissionKey { get; set; } = "";

        public StaffUser? User { get; set; }
    }
}

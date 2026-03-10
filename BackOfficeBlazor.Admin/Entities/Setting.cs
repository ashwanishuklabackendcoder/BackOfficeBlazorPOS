using System.ComponentModel.DataAnnotations;

namespace BackOfficeBlazor.Admin.Entities
{
    public class Setting
    {
        [Key]
        public int BranchId { get; set; }

        public string? SimConfig { get; set; }

        [Required]
        public string MadisonDealerId { get; set; } = string.Empty;
    }
}

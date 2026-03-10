using System.ComponentModel.DataAnnotations;

namespace BackOfficeBlazor.Admin.Entities;

public class QuickShortcutItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(10)]
    public string PartNumber { get; set; } = "";

    [Required]
    [StringLength(120)]
    public string Title { get; set; } = "";

    [StringLength(120)]
    public string? SubTitle { get; set; }

    [Required]
    [StringLength(16)]
    public string ColorHex { get; set; } = "#f8f9fa";

    [Required]
    [StringLength(16)]
    public string ForeColorHex { get; set; } = "#212529";

    public bool ShowAvailableStock { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}

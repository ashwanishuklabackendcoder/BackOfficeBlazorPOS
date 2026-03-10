namespace BackOfficeBlazor.Shared.DTOs;

public class QuickShortcutItemDto
{
    public int Id { get; set; }
    public string PartNumber { get; set; } = "";
    public string Title { get; set; } = "";
    public string? SubTitle { get; set; }
    public string ColorHex { get; set; } = "#f8f9fa";
    public string ForeColorHex { get; set; } = "#212529";
    public bool ShowAvailableStock { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public int? CurrentLocationStock { get; set; }
}

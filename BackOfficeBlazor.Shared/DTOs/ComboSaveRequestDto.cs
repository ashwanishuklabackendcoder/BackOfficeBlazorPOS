using System.Collections.Generic;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class ComboSaveRequestDto
    {
        public int? ComboId { get; set; }
        public string? ComboName { get; set; }
        public string? ComboPartNumber { get; set; }
        public decimal? ComboPrice { get; set; }
        public List<ComboDetailDto> Details { get; set; } = new();
    }
}

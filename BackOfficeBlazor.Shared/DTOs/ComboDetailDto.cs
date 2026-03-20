namespace BackOfficeBlazor.Shared.DTOs
{
    public class ComboDetailDto
    {
        public int ComboDetailId { get; set; }
        public int ComboId { get; set; }
        public string PartNumber { get; set; } = "";
        public string? ProductName { get; set; }
        public string? ImageMain { get; set; }
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? PromoPrice { get; set; }
        public decimal LineTotal { get; set; }
        public decimal PromoLineTotal { get; set; }
    }
}

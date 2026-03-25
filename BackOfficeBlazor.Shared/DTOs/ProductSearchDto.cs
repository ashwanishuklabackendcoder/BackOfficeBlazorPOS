namespace BackOfficeBlazor.Shared.DTOs
{
    public class ProductSearchDto
    {
        public string PartNumber { get; set; } = "";
        public string ShortDescription { get; set; } = "";
        public decimal StorePrice { get; set; }
        public decimal? PromoPrice { get; set; }
        public string? ImageMain { get; set; }
        public string? Image2 { get; set; }
        public string? Image3 { get; set; }
        public string? Image4 { get; set; }
    }
}

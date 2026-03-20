namespace BackOfficeBlazor.Shared.DTOs
{
    public class ComboInvoiceItemDto
    {
        public string PartNumber { get; set; } = "";
        public string ProductName { get; set; } = "";
        public int Qty { get; set; }
        public bool IsMajor { get; set; }
        public string? StockNumber { get; set; }
        public List<string> StockNumbers { get; set; } = new();
    }
}

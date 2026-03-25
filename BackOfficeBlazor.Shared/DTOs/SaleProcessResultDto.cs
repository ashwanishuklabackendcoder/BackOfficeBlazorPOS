namespace BackOfficeBlazor.Shared.DTOs
{
    public class SaleProcessResultDto
    {
        public string InvoiceNumber { get; set; } = "";
        public bool ReceiptPrintQueued { get; set; }
        public string ReceiptPrintMessage { get; set; } = "";
    }
}

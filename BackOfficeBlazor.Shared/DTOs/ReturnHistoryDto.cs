namespace BackOfficeBlazor.Shared.DTOs
{
    public class ReturnHistoryDto
    {
        public int Id { get; set; }
        public string InvoiceNo { get; set; } = "";
        public int? OriginalSaleLineId { get; set; }
        public string ProductId { get; set; } = "";
        public string ProductName { get; set; } = "";
        public int ReturnedQty { get; set; }
        public DateTime ReturnDate { get; set; }
        public string ReturnCondition { get; set; } = "";
        public string ReturnReason { get; set; } = "";
        public string StockMovementStatus { get; set; } = "";
    }
}

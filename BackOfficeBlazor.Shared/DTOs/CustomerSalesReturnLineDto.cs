using System;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class CustomerSalesReturnLineDto
    {
        public DateTime DateAndTime { get; set; }
        public string InvoiceNumber { get; set; } = "";
        public string PartNumber { get; set; } = "";
        public string StockNo { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Sell { get; set; }
        public decimal Net { get; set; }
        public decimal Vat { get; set; }
        public string InOut { get; set; } = ""; // "O" sale, "R" return
        public string Location { get; set; } = "";
        public string Terminal { get; set; } = "";
        public string Customer { get; set; } = "";
    }
}

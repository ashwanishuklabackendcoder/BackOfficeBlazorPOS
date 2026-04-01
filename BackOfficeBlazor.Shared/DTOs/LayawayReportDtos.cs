using System;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class LayawayReportRequestDto
    {
        public string? FromLocation { get; set; }
        public string? CategoryA { get; set; }
        public string? CategoryB { get; set; }
        public string? CategoryC { get; set; }
        public string ProductType { get; set; } = "Both";
        public string CustomerMode { get; set; } = "All";
        public string? CustomerAccNo { get; set; }
    }

    public class LayawayReportLineDto
    {
        public string CustomerAcc { get; set; } = string.Empty;
        public DateTime DateAndTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public string SalesCode { get; set; } = string.Empty;
        public string PartStockNo { get; set; } = string.Empty;
        public string Mfr { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Search1 { get; set; } = string.Empty;
        public string Search2 { get; set; } = string.Empty;
        public string CatA { get; set; } = string.Empty;
        public string CatB { get; set; } = string.Empty;
        public string CatC { get; set; } = string.Empty;
        public string Supplier { get; set; } = string.Empty;
        public int Qty { get; set; }
        public decimal Cost { get; set; }
        public decimal TotalCost { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public string Reserved { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string RefIdJobNo { get; set; } = string.Empty;
        public string DeliveryType { get; set; } = string.Empty;
    }
}

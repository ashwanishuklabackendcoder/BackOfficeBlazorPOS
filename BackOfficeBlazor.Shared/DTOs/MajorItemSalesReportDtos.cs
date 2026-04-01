using System;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class MajorItemSalesReportRequestDto
    {
        public string? Make { get; set; }
        public string? FromLocation { get; set; }
        public string? ToLocation { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? CategoryA { get; set; }
        public string? CategoryB { get; set; }
        public string? CategoryC { get; set; }
    }

    public class MajorItemSalesReportLineDto
    {
        public string Group { get; set; } = string.Empty;
        public int InStock { get; set; }
        public decimal InStockValue { get; set; }
        public int Sold { get; set; }
        public decimal SoldValue { get; set; }
        public decimal Profit { get; set; }
        public decimal MarginPercent { get; set; }
        public decimal Turnover { get; set; }
    }
}

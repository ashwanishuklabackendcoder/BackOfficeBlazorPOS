using System;
using System.Collections.Generic;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class StockPositionReportRequestDto
    {
        public string? FromLocation { get; set; }
        public string? ToLocation { get; set; }
        public int? FromYear { get; set; }
        public int? ToYear { get; set; }
        public bool CurrentOnly { get; set; }
        public string PrintMode { get; set; } = "Both";
        public bool OverstockOnly { get; set; }
        public bool UnderstockOnly { get; set; }
        public string? CategoryA { get; set; }
        public string? CategoryB { get; set; }
        public string? CategoryC { get; set; }
        public string? Supplier { get; set; }
    }

    public class StockPositionLineDto
    {
        public string PartNo { get; set; } = string.Empty;
        public string CategoryA { get; set; } = string.Empty;
        public string CategoryB { get; set; } = string.Empty;
        public string CategoryC { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public decimal RRP { get; set; }
        public decimal Promo { get; set; }
        public string Location { get; set; } = string.Empty;
        public int MIN { get; set; }
        public int MAX { get; set; }
        public int Stock { get; set; }
        public int StockDifference { get; set; }
    }
}

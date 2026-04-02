using System;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class PriceListReportRequestDto
    {
        public string? SupplierNo { get; set; }
        public string ProductMode { get; set; } = "Both";
        public bool PromoPriceOnly { get; set; }
        public bool NegativePriceOnly { get; set; }
        public string? FromLocation { get; set; }
        public string? ToLocation { get; set; }
        public string? Make { get; set; }
        public string? CategoryA { get; set; }
        public string? CategoryB { get; set; }
        public string? CategoryC { get; set; }
        public string? Year { get; set; }
        public string SortBy { get; set; } = "PartNumber";
    }

    public class PriceListReportLineDto
    {
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string CatA { get; set; } = string.Empty;
        public string CatB { get; set; } = string.Empty;
        public string CatC { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string PartNo { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string MfrPartNumber { get; set; } = string.Empty;
        public int TotalStock { get; set; }
        public decimal Price { get; set; }
        public decimal PromoPrice { get; set; }
        public DateTime? PromoStart { get; set; }
        public DateTime? PromoEnd { get; set; }
    }
}

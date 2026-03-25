using System;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class ProductEnquiryHeaderDto
    {
        public string PartNumber { get; set; } = string.Empty;
        public string? MfrPartNumber { get; set; }
        public string? Barcode { get; set; }
        public string? Make { get; set; }
        public string? SearchKeywords { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? BrandCode { get; set; }
        public string? Supplier1 { get; set; }
        public string? Supplier2 { get; set; }
        public decimal TotalStock { get; set; }
        public decimal LatestCost { get; set; }
        public decimal AverageCost { get; set; }
        public decimal? SuggestedRrp { get; set; }
        public DateTime? LastSoldDate { get; set; }
        public int TotalQtySold { get; set; }
        public decimal TotalSalesValue { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalVat { get; set; }
        public int FaultyStockQty { get; set; }
        public int BackOrdersQty { get; set; }
        public int MinStock { get; set; }
        public int MaxStock { get; set; }
        public int AllocatedStock { get; set; }
        public DateTime? LastStockCheck { get; set; }
        public string? CategoryHierarchy { get; set; }
        public decimal MarginPercent { get; set; }
        public string? ImageUrl { get; set; }
    }
}

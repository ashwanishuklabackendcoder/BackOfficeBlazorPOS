using System;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class StockTransferReportRequestDto
    {
        public string DateMode { get; set; } = "Today";
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? FromLocation { get; set; }
        public string? ToLocation { get; set; }
        public string Type { get; set; } = "Both";
        public string? CategoryA { get; set; }
        public string? CategoryB { get; set; }
        public string? CategoryC { get; set; }
    }

    public class StockTransferReportLineDto
    {
        public DateTime DateAndTime { get; set; }
        public string PartNo { get; set; } = string.Empty;
        public string MfrNo { get; set; } = string.Empty;
        public string LocFrom { get; set; } = string.Empty;
        public string LocTo { get; set; } = string.Empty;
        public decimal? Cost { get; set; }
        public int Qty { get; set; }
        public decimal Rrp { get; set; }
        public string StockNo { get; set; } = string.Empty;
        public string SerialNo { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Search1 { get; set; } = string.Empty;
        public string Search2 { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}

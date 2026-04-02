using System;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class MajorItemReportRequestDto
    {
        public string PrintBy { get; set; } = "InStock";
        public string? FromLocation { get; set; }
        public string? ToLocation { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string CategoryLevel { get; set; } = "A";
        public string? CategoryCode { get; set; }

        public string HeadingMode { get; set; } = "Make";
        public string? HeadingValue { get; set; }
    }

    public class MajorItemReportLineDto
    {
        public string StockNo { get; set; } = string.Empty;
        public string PartNo { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public decimal Rrp { get; set; }
        public decimal Promo { get; set; }
        public DateTime DateIn { get; set; }
        public string Size { get; set; } = string.Empty;
        public string Colour { get; set; } = string.Empty;
        public string Bin { get; set; } = string.Empty;
        public string FrameNumber { get; set; } = string.Empty;
        public string MfrSku { get; set; } = string.Empty;
    }
}

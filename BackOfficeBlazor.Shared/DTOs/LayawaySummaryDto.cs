using System;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class LayawaySummaryDto
    {
        public int LayawayNo { get; set; }
        public string CustomerAccNo { get; set; } = "";
        public DateTime DateCreated { get; set; }
        public decimal TotalNet { get; set; }
        public int ItemsCount { get; set; }
    }
}

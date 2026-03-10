using System;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class CustomerSalesReturnReportRequestDto
    {
        public string? CustomerAccNo { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public List<string> Locations { get; set; } = new();
    }
}

using System.Collections.Generic;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class BranchSettingsDto
    {
        public List<OpeningHourDto> OpeningHours { get; set; } = new();

        public bool EcommerceEnabled { get; set; }
        public string? EcommerceContactEmail { get; set; }
        public string? EcommerceOrderPrefix { get; set; }

        public string? PrintingReceiptHeader { get; set; }
        public string? PrintingReceiptFooter { get; set; }
        public string? PrintingPrinterName { get; set; }

        public bool WorkshopEnabled { get; set; }
        public string? WorkshopEmail { get; set; }
    }
}

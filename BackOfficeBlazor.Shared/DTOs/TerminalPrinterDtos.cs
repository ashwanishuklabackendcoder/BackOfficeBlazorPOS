using System.ComponentModel.DataAnnotations;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class TerminalPrinterAssignmentDto
    {
        [StringLength(1)]
        public string TerminalCode { get; set; } = "";

        [StringLength(2)]
        public string LocationCode { get; set; } = "";
        public int ReceiptPrinterId { get; set; }
        public int? A4PrinterId { get; set; }
        public int? LabelPrinterId { get; set; }
    }

    public class TerminalPrinterCatalogDto
    {
        [StringLength(1)]
        public string TerminalCode { get; set; } = "";

        [StringLength(2)]
        public string LocationCode { get; set; } = "";
        public List<PrinterListItemDto> Printers { get; set; } = new();
        public TerminalPrinterAssignmentDto Assignment { get; set; } = new();
    }
}

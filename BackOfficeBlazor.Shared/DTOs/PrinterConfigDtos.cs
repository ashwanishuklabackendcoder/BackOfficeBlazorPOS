using System.ComponentModel.DataAnnotations;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class PrinterConfigDto
    {
        public int Id { get; set; }

        [Required, StringLength(2)]
        public string LocationCode { get; set; } = "";

        [Required, StringLength(50)]
        public string PrinterName { get; set; } = "";

        [Required, StringLength(100)]
        public string Description { get; set; } = "";

        public int Mode { get; set; }
        public int Type { get; set; }
        public int LabelFormat { get; set; }

        [StringLength(16)]
        public string IpAddress { get; set; } = "";

        public int TcpPort { get; set; }

        [StringLength(20)]
        public string LocalPortName { get; set; } = "";

        public bool Shared { get; set; }

        [StringLength(20)]
        public string ShareHostAddress { get; set; } = "";

        [StringLength(50)]
        public string ShareName { get; set; } = "";

        [StringLength(100)]
        public string ShareUsername { get; set; } = "";

        [StringLength(100)]
        public string SharePassword { get; set; } = "";

        public bool ReceiptPrintLogo { get; set; }
        public int ReceiptLogoSize { get; set; }

        [StringLength(100)]
        public string Filename { get; set; } = "";

        [StringLength(5)]
        public string FileExtension { get; set; } = "";

        public bool FileEnsureUnique { get; set; }

        [StringLength(250)]
        public string EmailDestination { get; set; } = "";

        public bool IsOnline { get; set; }
        public DateTime? LastPrintedAt { get; set; }
    }

    public class PrinterListItemDto
    {
        public int Id { get; set; }
        public string LocationCode { get; set; } = "";
        public string PrinterName { get; set; } = "";
        public int Type { get; set; }
        public int Mode { get; set; }
        public string IpAddress { get; set; } = "";
        public bool IsOnline { get; set; }
        public DateTime? LastPrintedAt { get; set; }
    }

    public class PrinterTestRequestDto
    {
        [Required]
        [StringLength(1)]
        public string TerminalCode { get; set; } = "";
    }

    public class PrinterValidationResultDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = "";
    }
}

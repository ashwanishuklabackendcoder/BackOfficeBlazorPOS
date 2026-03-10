using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackOfficeBlazor.Admin.Entities
{
    [Table("PrinterConfigs")]
    public class PrinterConfig
    {
        [Key]
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

        [Required, StringLength(16)]
        public string IpAddress { get; set; } = "";

        public int TcpPort { get; set; }

        [Required, StringLength(20)]
        public string LocalPortName { get; set; } = "";

        public bool Shared { get; set; }

        [Required, StringLength(20)]
        public string ShareHostAddress { get; set; } = "";

        [Required, StringLength(50)]
        public string ShareName { get; set; } = "";

        [Required, StringLength(100)]
        public string ShareUsername { get; set; } = "";

        [Required, StringLength(100)]
        public string SharePassword { get; set; } = "";

        public bool ReceiptPrintLogo { get; set; }
        public int ReceiptLogoSize { get; set; }

        [Required, StringLength(100)]
        public string Filename { get; set; } = "";

        [Required, StringLength(5)]
        public string FileExtension { get; set; } = "";

        public bool FileEnsureUnique { get; set; }

        [Required, StringLength(250)]
        public string EmailDestination { get; set; } = "";

        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
}

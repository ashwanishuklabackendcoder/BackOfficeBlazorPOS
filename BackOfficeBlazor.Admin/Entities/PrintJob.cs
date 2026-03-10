using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackOfficeBlazor.Admin.Entities
{
    [Table("PrintJobs")]
    public class PrintJob
    {
        [Key]
        public int Id { get; set; }

        [StringLength(5)]
        public string TerminalCode { get; set; } = "";

        public int PrinterConfigId { get; set; }

        [StringLength(20)]
        public string JobType { get; set; } = "";

        public string Payload { get; set; } = "";

        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}

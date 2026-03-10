using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackOfficeBlazor.Admin.Entities
{
    [Table("TerminalOptions")]
    public class TerminalOption
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        [Required, StringLength(1)]
        public string TerminalCode { get; set; } = "";

        [StringLength(2)]
        public string? DefaultBranch { get; set; }

        public int ReceiptPrinterId { get; set; }
        public int? A4PrinterId { get; set; }
        public int? LabelPrinterId { get; set; }
    }
}

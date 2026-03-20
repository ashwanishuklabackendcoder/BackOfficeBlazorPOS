using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackOfficeBlazor.Admin.Entities
{
    [Table("ComboDetail")]
    public class ComboDetail
    {
        [Key]
        public int ComboDetailId { get; set; }
        public int ComboId { get; set; }
        [StringLength(10)]
        public string PartNumber { get; set; } = "";
        [StringLength(500)]
        public string? ProductName { get; set; }
        public string? ImageMain { get; set; }
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? PromoPrice { get; set; }
        public decimal LineTotal { get; set; }
        public decimal PromoLineTotal { get; set; }
        public ComboMaster? Combo { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackOfficeBlazor.Admin.Entities
{
    [Table("ReturnFaultyItems")]
    public class ReturnFaultyItem
    {
        [Key]
        public int Id { get; set; }

        [StringLength(30)]
        public string InvoiceNo { get; set; } = string.Empty;

        [StringLength(20)]
        public string ProductId { get; set; } = string.Empty;

        public int Qty { get; set; }

        [StringLength(250)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(50)]
        public string Condition { get; set; } = string.Empty;

        public DateTime ReturnDate { get; set; }
        public DateTime? SaleDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SaleAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ReturnAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [StringLength(20)]
        public string CustomerAccount { get; set; } = string.Empty;

        [StringLength(20)]
        public string SalesCode { get; set; } = string.Empty;

        [StringLength(2)]
        public string StoreId { get; set; } = string.Empty;

        [StringLength(50)]
        public string CreatedBy { get; set; } = string.Empty;

        public int? ReferenceReturnId { get; set; }
    }
}

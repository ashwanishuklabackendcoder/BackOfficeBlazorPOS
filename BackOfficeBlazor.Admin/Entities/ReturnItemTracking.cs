using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackOfficeBlazor.Admin.Entities
{
    [Table("ReturnItemTracking")]
    public class ReturnItemTracking
    {
        [Key]
        public int Id { get; set; }

        [StringLength(30)]
        public string InvoiceNo { get; set; } = string.Empty;

        public int? OriginalSaleLineId { get; set; }

        [StringLength(20)]
        public string ProductId { get; set; } = string.Empty;

        [StringLength(500)]
        public string ProductName { get; set; } = string.Empty;

        public int Qty { get; set; }

        [StringLength(250)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(50)]
        public string Condition { get; set; } = string.Empty;

        public DateTime ReturnDate { get; set; }

        [StringLength(60)]
        public string StockMovementStatus { get; set; } = string.Empty;

        [StringLength(2)]
        public string StoreId { get; set; } = string.Empty;

        [StringLength(50)]
        public string CreatedBy { get; set; } = string.Empty;

        public int? ReferenceReturnId { get; set; }
    }
}

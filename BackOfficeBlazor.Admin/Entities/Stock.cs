using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Entities
{
    public class Stock
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(5)]
        public string PartNumber { get; set; } = null!;

        [Required]
        [StringLength(2)]
        public string LocationCode { get; set; } = null!;

        [Required]
        [StringLength(10)]
        public string StockNumber { get; set; } = null!;

        [Required]
        [StringLength(10)]
        public string SerialNumber { get; set; } = null!;

        [Required]
        [StringLength(6)]
        public string CustomerAccNo { get; set; } = null!;

        [Required]
        [StringLength(2)]
        public string StaffCode { get; set; } = null!;

        [Required]
        [StringLength(6)]
        public string SupplierCode { get; set; } = null!;

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Cost { get; set; }

        [Required]
        [StringLength(15)]
        public string InvoiceNumber { get; set; } = null!;

        [Required]
        [StringLength(15)]
        public string PurchaseOrderNo { get; set; } = null!;

        [Required]
        public int PrintLabelOption { get; set; }

        public int? StockItemStatus { get; set; }

        [Required]
        public bool IsPrinted { get; set; }

        [Required]
        public bool IsAvailable { get; set; }

        [Required]
        public bool IsCollected { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        public DateTime? DateUpdated { get; set; }
    }
}

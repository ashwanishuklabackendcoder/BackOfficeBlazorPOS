using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackOfficeBlazor.Admin.Entities
{
    [Table("_PurchaseOrderItems")]
    public class PurchaseOrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(10)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public int SequenceId { get; set; }

        [Required, StringLength(5)]
        public string PartNumber { get; set; } = string.Empty;

        [Required, StringLength(25)]
        public string MfrPartNumber { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int BoxQty { get; set; }

        [Required]
        public int QtyRequired { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal CostPrice { get; set; }

        [Required]
        public int QtyRecieved { get; set; }

        [Required, StringLength(2)]
        public string StockLocationCode { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string DeliveryLocationCode { get; set; } = string.Empty;

        [Required, StringLength(6)]
        public string CustomerAccNo { get; set; } = string.Empty;

        [Required]
        public bool IsMajor { get; set; }

        [Required]
        public bool XmasClub { get; set; }

        [Required]
        public bool SmsCustomerOnArrival { get; set; }

        [Required, StringLength(100)]
        public string Notes { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string OrderedByCode { get; set; } = string.Empty;

        [Required, StringLength(6)]
        public string SupplierCode { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedOnDate { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public DateTime DateCreated { get; set; }

        public DateTime? DateUpdated { get; set; }

        [Required]
        public int InternalOrderRefID { get; set; }
    }
}

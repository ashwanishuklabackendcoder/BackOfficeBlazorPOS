using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackOfficeBlazor.Admin.Entities
{
    [Table("_PurchaseOrderHeaders")]
    public class PurchaseOrderHeader
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(10)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required, StringLength(25)]
        public string RaisedByStaffCode { get; set; } = string.Empty;

        [Required]
        public DateTime RaisedOnDate { get; set; }

        [Required]
        public double CarriageCost { get; set; }

        [StringLength(2)]
        public string? AmendedLastByCode { get; set; }

        public DateTime? AmendedLastOnDate { get; set; }

        [StringLength(2)]
        public string? ClosedByCode { get; set; }

        public DateTime? ClosedOnDate { get; set; }

        [StringLength(2)]
        public string? CancelledByCode { get; set; }

        public DateTime? CancelledOnDate { get; set; }

        [Required]
        public int Status { get; set; }

        [Required, StringLength(6)]
        public string SupplierCode { get; set; } = string.Empty;

        [Required]
        public bool IsImported { get; set; }

        public string? JsonReport { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        public DateTime? DateUpdated { get; set; }

        [Required]
        public bool DirectToStore { get; set; }
    }
}

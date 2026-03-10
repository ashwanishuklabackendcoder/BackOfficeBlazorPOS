using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class StockInputDto
    {
        // -----------------------------
        // PRODUCT
        // -----------------------------
        [StringLength(10)]
        public string PartNumber { get; set; } = null!;

        /// <summary>
        /// Copied from Product.Major at time of save (do NOT trust client blindly)
        /// </summary>
        public bool IsMajorProduct { get; set; }

        // -----------------------------
        // LOCATION & QUANTITY
        // -----------------------------
        [StringLength(2)]
        public string LocationCode { get; set; } = null!; // "01", "02", etc.

        /// <summary>
        /// Quantity entered by admin.
        /// For Major: number of units
        /// For Minor: bulk quantity
        /// </summary>
        public int Quantity { get; set; }

        public int? BoxQuantity { get; set; }

        public DateTime DateIn { get; set; }

        // -----------------------------
        // COMMERCIAL
        // -----------------------------
        [StringLength(15)]
        public string? InvoiceNumber { get; set; }

        [StringLength(15)]
        public string? PurchaseOrderNo { get; set; }

        public decimal CostEach { get; set; }

        public decimal TotalCost => Quantity * CostEach;

        [StringLength(6)]
        public string SupplierCode { get; set; } = null!;

        // -----------------------------
        // OPERATOR / META
        // -----------------------------
        [StringLength(2)]
        public string StaffCode { get; set; } = null!;

        public bool PrintLabel { get; set; }

        /// <summary>
        /// Optional free text for audit / future
        /// </summary>
        public string? Remarks { get; set; }
    }

}

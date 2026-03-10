using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class ProductStockCreateDto
    {
        public string PartNumber { get; set; } = null!;
        public string LocationCode { get; set; } = null!;

        /// <summary>
        /// NULL for Minor products
        /// Generated for Major products
        /// </summary>
        public string? StockNumber { get; set; }

        public string? SerialNumber { get; set; }

        public int Quantity { get; set; } // 1 for Major, N for Minor

        public decimal Cost { get; set; }

        public string SupplierCode { get; set; } = null!;
        public string? InvoiceNumber { get; set; }
        public string? PurchaseOrderNo { get; set; }

        public bool IsPrinted { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsCollected { get; set; }

        public DateTime DateCreated { get; set; }
        public string StaffCode { get; set; } = null!;
    }

}

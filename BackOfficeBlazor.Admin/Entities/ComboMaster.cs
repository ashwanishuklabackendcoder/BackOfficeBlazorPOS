using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BackOfficeBlazor.Admin.Entities
{
    [Table("ComboMaster")]
    public class ComboMaster
    {
        [Key]
        public int ComboId { get; set; }
        [StringLength(10)]
        public string ComboPartNumber { get; set; } = "";
        [StringLength(200)]
        public string ComboName { get; set; } = "";
        public int NumberOfProductsIncluded { get; set; }
        public int TotalQty { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal OfferPrice { get; set; }
        public decimal ComboPrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime? UpdatedOn { get; set; }
        public List<ComboDetail> Details { get; set; } = new();
    }
}

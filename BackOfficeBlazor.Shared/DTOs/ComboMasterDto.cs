using System;
using System.Collections.Generic;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class ComboMasterDto
    {
        public int ComboId { get; set; }
        public string ComboPartNumber { get; set; } = "";
        public string ComboName { get; set; } = "";
        public int NumberOfProductsIncluded { get; set; }
        public int TotalQty { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal OfferPrice { get; set; }
        public decimal ComboPrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public List<ComboDetailDto> Details { get; set; } = new();
    }
}

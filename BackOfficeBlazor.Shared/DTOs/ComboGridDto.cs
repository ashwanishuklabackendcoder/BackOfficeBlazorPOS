using System;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class ComboGridDto
    {
        public int ComboId { get; set; }
        public string ComboPartNumber { get; set; } = "";
        public string ComboName { get; set; } = "";
        public int ProductsCount { get; set; }
        public int TotalQty { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal OfferPrice { get; set; }
        public decimal ComboPrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}

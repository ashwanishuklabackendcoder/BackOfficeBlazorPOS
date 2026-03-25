using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class PosSaleLineDto
    {
        public int SaleLineId { get; set; }
        public string PartNumber { get; set; } = "";
        public string Description { get; set; } = "";
        public string StockNo { get; set; } = "";   // Major only
        public string Terminal { get; set; } = "";
        public string Location { get; set; } = "";
        public int Quantity { get; set; }

        public decimal Cost { get; set; }
        public decimal Sell { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }

        public decimal Vat { get; set; }
        public decimal Net { get; set; }
        public bool IsMajor { get; set; }
        public bool IsCombo { get; set; }
        public int? ComboId { get; set; }
        public string? ComboGroupId { get; set; }
        public bool IsComboReturnPolicyApplied { get; set; }
        public decimal RefundableAmount { get; set; }
        public string? Make { get; set; }
        public List<ComboInvoiceItemDto> ComboItems { get; set; } = new();

    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class ReturnLineDto
    {
        public int SaleLineId { get; set; }
        public string PartNumber { get; set; } = "";
        public string Description { get; set; } = "";

        public string? StockNo { get; set; }   // For Major Items
        public bool IsCombo { get; set; }
        public int? ComboId { get; set; }
        public string? ComboGroupId { get; set; }
        public bool IsComboReturnPolicyApplied { get; set; }

        public int Qty { get; set; }
        public string Location { get; set; } = "";   // ⭐ ADD THIS

        public string Terminal { get; set; } = "";
        public decimal Sell { get; set; }
        public decimal RefundAmount { get; set; }

        public string Reason { get; set; } = "";
        public string Condition { get; set; } = ReturnConditions.OkSellable;
    }
}


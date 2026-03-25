using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.State
{
    public static class PosWorkflowState
    {
        public static List<PosCartItemState> PosCart { get; set; } = new();
        public static string PosCustomerAccount { get; set; } = "WALKIN";
        public static string PosCustomerName { get; set; } = "Walk In";
        public static string PosLocation { get; set; } = "01";
        public static string? PendingProductSearchReturnUri { get; set; }
        public static ProductDto? PendingProductSelection { get; set; }

        public static string LayawayCustomerAccount { get; set; } = "WALKIN";
        public static string LayawayCustomerName { get; set; } = "Walk In";
        public static List<PosCartItemState> LayawayDraftCart { get; set; } = new();

        public static PendingLayawayToSellState? PendingLayawayToSell { get; set; }
    }

    public class PosCartItemState
    {
        public string PartNumber { get; set; } = "";
        public string Description { get; set; } = "";
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public bool AllowDiscount { get; set; } = true;
        public DiscountMode DiscountMode { get; set; } = DiscountMode.Percent;
        public bool IsMajor { get; set; }
        public bool IsCombo { get; set; }
        public int? ComboId { get; set; }
        public string? ComboGroupId { get; set; }
        public bool IsComboReturnPolicyApplied { get; set; }
        public string? StockNumber { get; set; }
        public List<ComboInvoiceItemDto> ComboItems { get; set; } = new();
    }

    public enum DiscountMode
    {
        Percent = 0,
        Amount = 1
    }

    public class PendingLayawayToSellState
    {
        public int LayawayNo { get; set; }
        public string CustomerAccNo { get; set; } = "WALKIN";
        public string CustomerName { get; set; } = "Walk In";
        public string Location { get; set; } = "01";
        public List<LayawayLineDto> Lines { get; set; } = new();
    }
}

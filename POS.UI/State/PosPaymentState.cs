using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.State
{
    public static class PosPaymentState
    {
        public static PosSaleRequestDto? CurrentSale { get; set; }
        public static ReturnProcessDto? CurrentReturn { get; set; }
        public static int? CurrentLayawayNo { get; set; }
    }

}

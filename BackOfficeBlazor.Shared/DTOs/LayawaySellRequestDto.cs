namespace BackOfficeBlazor.Shared.DTOs
{
    public class LayawaySellRequestDto
    {
        public int LayawayNo { get; set; }
        public PosPaymentDto Payment { get; set; } = new();
    }
}

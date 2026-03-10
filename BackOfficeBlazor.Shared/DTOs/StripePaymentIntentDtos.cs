namespace BackOfficeBlazor.Shared.DTOs
{
    public class StripeKeyValidationRequestDto
    {
        public string SecretKey { get; set; } = "";
    }

    public class StripePaymentIntentRequestDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "eur";
        public string? Reference { get; set; }
    }

    public class StripePaymentIntentResponseDto
    {
        public string ClientSecret { get; set; } = "";
        public string PaymentIntentId { get; set; } = "";
    }

    public class StripePaymentVerifyRequestDto
    {
        public string PaymentIntentId { get; set; } = "";
    }
}

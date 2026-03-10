using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public interface IPaymentService
    {
        Task<string> ProcessSaleAsync(PosSaleRequestDto dto);
        Task<ApiResponse<StripePaymentIntentResponseDto>> CreateStripePaymentIntentAsync(
            StripePaymentIntentRequestDto dto);
        Task<ApiResponse<bool>> VerifyStripePaymentAsync(StripePaymentVerifyRequestDto dto);
    }

}

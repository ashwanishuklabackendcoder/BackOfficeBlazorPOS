using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public interface IPaymentService
    {
        Task<ApiResponse<SaleProcessResultDto>> ProcessSaleAsync(PosSaleRequestDto dto);
        Task<ApiResponse<StripePaymentIntentResponseDto>> CreateStripePaymentIntentAsync(
            StripePaymentIntentRequestDto dto);
        Task<ApiResponse<bool>> VerifyStripePaymentAsync(StripePaymentVerifyRequestDto dto);
    }
}

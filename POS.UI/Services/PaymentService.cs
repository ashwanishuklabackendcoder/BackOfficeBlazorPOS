using BackOfficeBlazor.Shared.DTOs;
using System.Net.Http.Json;

namespace POS.UI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _http;

        public PaymentService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ApiResponse<SaleProcessResultDto>> ProcessSaleAsync(PosSaleRequestDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/sales/process", dto);
            var payload = await ApiResponseReader.ReadAsync<SaleProcessResultDto>(
                response,
                "Sale could not be completed.");

            if (!response.IsSuccessStatusCode)
                return ApiResponse<SaleProcessResultDto>.Fail(payload.Message);

            if (!payload.Success || payload.Data == null || string.IsNullOrWhiteSpace(payload.Data.InvoiceNumber))
            {
                return ApiResponse<SaleProcessResultDto>.Fail(
                    string.IsNullOrWhiteSpace(payload.Message)
                        ? "Sale could not be completed."
                        : payload.Message);
            }

            return payload;
        }

        public async Task<ApiResponse<StripePaymentIntentResponseDto>> CreateStripePaymentIntentAsync(
            StripePaymentIntentRequestDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/payments/create-payment-intent", dto);
            var payload = await ApiResponseReader.ReadAsync<StripePaymentIntentResponseDto>(
                response,
                "Stripe intent failed.");

            return response.IsSuccessStatusCode
                ? payload
                : ApiResponse<StripePaymentIntentResponseDto>.Fail(payload.Message);
        }

        public async Task<ApiResponse<bool>> VerifyStripePaymentAsync(StripePaymentVerifyRequestDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/payments/verify-payment-intent", dto);
            var payload = await ApiResponseReader.ReadAsync<bool>(
                response,
                "Stripe verification failed.");

            return response.IsSuccessStatusCode
                ? payload
                : ApiResponse<bool>.Fail(payload.Message);
        }
    }
}

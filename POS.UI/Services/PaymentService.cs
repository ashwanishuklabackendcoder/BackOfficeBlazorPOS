using BackOfficeBlazor.Shared.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace POS.UI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _http;

        public PaymentService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> ProcessSaleAsync(PosSaleRequestDto dto)
        {
            var res = await _http.PostAsJsonAsync("api/sales/process", dto);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                throw new Exception(BuildErrorMessage((int)res.StatusCode, body));
            }

            var json = await res.Content.ReadFromJsonAsync<ProcessSaleResponseDto>();
            if (json == null || string.IsNullOrWhiteSpace(json.InvoiceNumber))
                throw new Exception("Sale failed: invalid response from server.");

            return json.InvoiceNumber;
        }

        public async Task<ApiResponse<StripePaymentIntentResponseDto>> CreateStripePaymentIntentAsync(
            StripePaymentIntentRequestDto dto)
        {
            var res = await _http.PostAsJsonAsync("api/payments/create-payment-intent", dto);

            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                return ApiResponse<StripePaymentIntentResponseDto>.Fail(
                    string.IsNullOrWhiteSpace(body) ? "Stripe intent failed." : body);
            }

            return await res.Content.ReadFromJsonAsync<ApiResponse<StripePaymentIntentResponseDto>>()
                   ?? ApiResponse<StripePaymentIntentResponseDto>.Fail("Null response");
        }

        public async Task<ApiResponse<bool>> VerifyStripePaymentAsync(StripePaymentVerifyRequestDto dto)
        {
            var res = await _http.PostAsJsonAsync("api/payments/verify-payment-intent", dto);

            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                return ApiResponse<bool>.Fail(
                    string.IsNullOrWhiteSpace(body) ? "Stripe verification failed." : body);
            }

            return await res.Content.ReadFromJsonAsync<ApiResponse<bool>>()
                   ?? ApiResponse<bool>.Fail("Null response");
        }

        private static string BuildErrorMessage(int statusCode, string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return $"Sale failed ({statusCode}).";

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (root.TryGetProperty("error", out var error))
                    return $"Sale failed ({statusCode}): {error.GetString()}";
                if (root.TryGetProperty("message", out var message))
                    return $"Sale failed ({statusCode}): {message.GetString()}";
            }
            catch
            {
                // Non-JSON payload.
            }

            var preview = body.Length > 220 ? body[..220] + "..." : body;
            return $"Sale failed ({statusCode}): {preview}";
        }

        class ProcessSaleResponseDto
        {
            public string InvoiceNumber { get; set; } = "";
        }
    }
}

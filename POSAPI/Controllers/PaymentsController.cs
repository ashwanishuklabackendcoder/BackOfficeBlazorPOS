using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using POSAPI.Services;

namespace POSAPI.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IStripeService _stripe;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IStripeService stripe, ILogger<PaymentsController> logger)
        {
            _stripe = stripe;
            _logger = logger;
        }

        [HttpPost("validate-key")]
        public async Task<IActionResult> ValidateKey([FromBody] StripeKeyValidationRequestDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.SecretKey))
                return BadRequest(ApiResponse<bool>.Fail("Secret key is required."));

            var valid = await _stripe.ValidateKeyAsync(dto.SecretKey);

            return Ok(new ApiResponse<bool>
            {
                Success = valid,
                Data = valid,
                Message = valid ? "Key is valid." : "Key is invalid."
            });
        }

        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] StripePaymentIntentRequestDto dto)
        {
            if (dto == null)
                return BadRequest(ApiResponse<StripePaymentIntentResponseDto>.Fail("Invalid payload."));

            var amount = (long)Math.Round(dto.Amount * 100m, 0, MidpointRounding.AwayFromZero);
            var currency = string.IsNullOrWhiteSpace(dto.Currency) ? "eur" : dto.Currency.Trim().ToLowerInvariant();

            var result = await _stripe.CreatePaymentIntentAsync(amount, currency);
            if (!result.Success || string.IsNullOrWhiteSpace(result.ClientSecret))
            {
                _logger.LogWarning("Payment intent failed: {Message}", result.Message);
                return BadRequest(ApiResponse<StripePaymentIntentResponseDto>.Fail(result.Message));
            }

            return Ok(new ApiResponse<StripePaymentIntentResponseDto>
            {
                Success = true,
                Data = new StripePaymentIntentResponseDto
                {
                    ClientSecret = result.ClientSecret ?? "",
                    PaymentIntentId = ""
                },
                Message = "Payment intent created"
            });
        }

        [HttpPost("verify-payment-intent")]
        public async Task<IActionResult> VerifyPaymentIntent([FromBody] StripePaymentVerifyRequestDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.PaymentIntentId))
                return BadRequest(ApiResponse<bool>.Fail("Payment intent id is required."));

            var result = await _stripe.VerifyPaymentIntentAsync(dto.PaymentIntentId);
            if (!result.Success)
                return BadRequest(ApiResponse<bool>.Fail(result.Message));

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = result.Message
            });
        }
    }
}

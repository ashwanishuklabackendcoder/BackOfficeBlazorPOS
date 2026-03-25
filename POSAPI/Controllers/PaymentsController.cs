using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSAPI.Security;
using POSAPI.Services;

namespace POSAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IStripeService _stripe;
        private readonly ApiAccessService _access;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IStripeService stripe,
            ApiAccessService access,
            ILogger<PaymentsController> logger)
        {
            _stripe = stripe;
            _access = access;
            _logger = logger;
        }

        [HttpPost("validate-key")]
        public async Task<IActionResult> ValidateKey([FromBody] StripeKeyValidationRequestDto dto)
        {
            if (!_access.IsAdmin(User))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<bool>.Fail("You do not have permission to validate payment keys."));
            }

            if (dto == null || string.IsNullOrWhiteSpace(dto.SecretKey))
                return BadRequest(ApiResponse<bool>.Fail("Secret key is required."));

            try
            {
                var valid = await _stripe.ValidateKeyAsync(dto.SecretKey);
                return Ok(ApiResponse<bool>.Ok(valid, valid ? "Key is valid." : "Key is invalid."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stripe key validation failed.");
                return BadRequest(ApiResponse<bool>.Fail("Unable to validate the payment key right now."));
            }
        }

        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] StripePaymentIntentRequestDto dto)
        {
            if (!await _access.HasAnyPermissionAsync(User, PermissionKeys.Till))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<StripePaymentIntentResponseDto>.Fail("You do not have permission to take card payments."));
            }

            if (dto == null)
                return BadRequest(ApiResponse<StripePaymentIntentResponseDto>.Fail("Invalid payload."));

            if (dto.Amount <= 0)
                return BadRequest(ApiResponse<StripePaymentIntentResponseDto>.Fail("Payment amount must be greater than zero."));

            var amount = (long)Math.Round(dto.Amount * 100m, 0, MidpointRounding.AwayFromZero);
            var currency = string.IsNullOrWhiteSpace(dto.Currency) ? "eur" : dto.Currency.Trim().ToLowerInvariant();

            try
            {
                var result = await _stripe.CreatePaymentIntentAsync(amount, currency);
                if (!result.Success || string.IsNullOrWhiteSpace(result.ClientSecret))
                {
                    _logger.LogWarning("Payment intent failed: {Message}", result.Message);
                    return BadRequest(ApiResponse<StripePaymentIntentResponseDto>.Fail(
                        string.IsNullOrWhiteSpace(result.Message)
                            ? "Unable to start card payment."
                            : result.Message));
                }

                return Ok(ApiResponse<StripePaymentIntentResponseDto>.Ok(
                    new StripePaymentIntentResponseDto
                    {
                        ClientSecret = result.ClientSecret ?? "",
                        PaymentIntentId = ""
                    },
                    "Payment intent created."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment intent creation failed.");
                return BadRequest(ApiResponse<StripePaymentIntentResponseDto>.Fail(
                    "Unable to start card payment right now."));
            }
        }

        [HttpPost("verify-payment-intent")]
        public async Task<IActionResult> VerifyPaymentIntent([FromBody] StripePaymentVerifyRequestDto dto)
        {
            if (!await _access.HasAnyPermissionAsync(User, PermissionKeys.Till))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<bool>.Fail("You do not have permission to verify card payments."));
            }

            if (dto == null || string.IsNullOrWhiteSpace(dto.PaymentIntentId))
                return BadRequest(ApiResponse<bool>.Fail("Payment intent id is required."));

            try
            {
                var result = await _stripe.VerifyPaymentIntentAsync(dto.PaymentIntentId);
                if (!result.Success)
                    return BadRequest(ApiResponse<bool>.Fail(result.Message));

                return Ok(ApiResponse<bool>.Ok(true, result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment intent verification failed for {PaymentIntentId}", dto.PaymentIntentId);
                return BadRequest(ApiResponse<bool>.Fail("Unable to verify card payment right now."));
            }
        }
    }
}

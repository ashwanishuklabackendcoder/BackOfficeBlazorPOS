using Microsoft.Extensions.Options;
using POSAPI.Config;
using Stripe;

namespace POSAPI.Services
{
    public interface IStripeService
    {
        Task<bool> ValidateKeyAsync(string secretKey);
        Task<(bool Success, string Message, string? ClientSecret)> CreatePaymentIntentAsync(long amount, string currency);
        Task<(bool Success, string Message)> VerifyPaymentIntentAsync(string paymentIntentId);
    }

    public class StripeService : IStripeService
    {
        private readonly StripeSettings _settings;
        private readonly ILogger<StripeService> _logger;

        public StripeService(IOptions<StripeSettings> settings, ILogger<StripeService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<bool> ValidateKeyAsync(string secretKey)
        {
            if (string.IsNullOrWhiteSpace(secretKey))
                return false;
            if (secretKey.StartsWith("SET_STRIPE", StringComparison.OrdinalIgnoreCase))
                return false;

            try
            {
                StripeConfiguration.ApiKey = secretKey;
                var service = new BalanceService();
                _ = await service.GetAsync();
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogWarning("Stripe key validation failed: {Message}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error validating Stripe key.");
                return false;
            }
        }

        public async Task<(bool Success, string Message, string? ClientSecret)> CreatePaymentIntentAsync(long amount, string currency)
        {
            if (string.IsNullOrWhiteSpace(_settings.SecretKey))
                return (false, "Stripe secret key is not configured.", null);
            if (_settings.SecretKey.StartsWith("SET_STRIPE", StringComparison.OrdinalIgnoreCase))
                return (false, "Stripe secret key is not configured.", null);

            if (amount <= 0)
                return (false, "Amount must be greater than zero.", null);

            try
            {
                StripeConfiguration.ApiKey = _settings.SecretKey;

                var options = new PaymentIntentCreateOptions
                {
                    Amount = amount,
                    Currency = string.IsNullOrWhiteSpace(currency) ? "eur" : currency.Trim().ToLowerInvariant(),
                    PaymentMethodTypes = new List<string> { "card" }
                };

                var service = new PaymentIntentService();
                var intent = await service.CreateAsync(options);

                return (true, "Payment intent created", intent.ClientSecret);
            }
            catch (StripeException ex)
            {
                _logger.LogError("Stripe error creating PaymentIntent: {Message}", ex.Message);
                return (false, "Stripe payment failed.", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating PaymentIntent.");
                return (false, "Internal server error.", null);
            }
        }

        public async Task<(bool Success, string Message)> VerifyPaymentIntentAsync(string paymentIntentId)
        {
            if (string.IsNullOrWhiteSpace(_settings.SecretKey))
                return (false, "Stripe secret key is not configured.");
            if (_settings.SecretKey.StartsWith("SET_STRIPE", StringComparison.OrdinalIgnoreCase))
                return (false, "Stripe secret key is not configured.");
            if (string.IsNullOrWhiteSpace(paymentIntentId))
                return (false, "Payment intent id is required.");

            try
            {
                StripeConfiguration.ApiKey = _settings.SecretKey;

                var service = new PaymentIntentService();
                var intent = await service.GetAsync(paymentIntentId);

                if (!string.Equals(intent.Status, "succeeded", StringComparison.OrdinalIgnoreCase))
                    return (false, $"Payment not completed. Status: {intent.Status}");

                return (true, "Payment verified");
            }
            catch (StripeException ex)
            {
                _logger.LogError("Stripe error verifying PaymentIntent: {Message}", ex.Message);
                return (false, "Stripe verification failed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error verifying PaymentIntent.");
                return (false, "Internal server error.");
            }
        }
    }
}

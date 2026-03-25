using System.Net.Http.Json;
using System.Text.Json;
using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    internal static class ApiResponseReader
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task<ApiResponse<T>> ReadAsync<T>(HttpResponseMessage response, string fallbackMessage)
        {
            try
            {
                var payload = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
                if (payload != null)
                    return payload;
            }
            catch
            {
            }

            var raw = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(raw))
                return ApiResponse<T>.Fail(fallbackMessage);

            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("message", out var messageElement))
                {
                    var message = messageElement.GetString();
                    if (!string.IsNullOrWhiteSpace(message))
                        return ApiResponse<T>.Fail(message);
                }
            }
            catch
            {
            }

            return ApiResponse<T>.Fail(raw);
        }
    }
}

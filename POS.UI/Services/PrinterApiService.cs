using BackOfficeBlazor.Shared.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace POS.UI.Services
{
    public class PrinterApiService : IPrinterApiService
    {
        private readonly HttpClient _http;

        public PrinterApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ApiResponse<List<PrinterListItemDto>>> GetPrintersAsync(string locationCode)
            => await SendAsync<List<PrinterListItemDto>>(HttpMethod.Get, $"api/printers?locationCode={locationCode}");

        public async Task<ApiResponse<PrinterConfigDto>> GetPrinterAsync(int id)
            => await SendAsync<PrinterConfigDto>(HttpMethod.Get, $"api/printers/{id}");

        public async Task<ApiResponse<PrinterConfigDto>> CreatePrinterAsync(PrinterConfigDto dto)
            => await SendAsync<PrinterConfigDto>(HttpMethod.Post, "api/printers", dto);

        public async Task<ApiResponse<PrinterConfigDto>> UpdatePrinterAsync(PrinterConfigDto dto)
            => await SendAsync<PrinterConfigDto>(HttpMethod.Put, $"api/printers/{dto.Id}", dto);

        public async Task<ApiResponse<bool>> DeletePrinterAsync(int id)
            => await SendAsync<bool>(HttpMethod.Delete, $"api/printers/{id}");

        public async Task<ApiResponse<PrintJobDto>> TestPrinterAsync(int id, string terminalCode)
            => await SendAsync<PrintJobDto>(
                HttpMethod.Post,
                $"api/printers/test/{id}",
                new PrinterTestRequestDto { TerminalCode = terminalCode });

        public async Task<ApiResponse<TerminalPrinterCatalogDto>> GetTerminalPrintersAsync(string terminalCode, string locationCode)
            => await SendAsync<TerminalPrinterCatalogDto>(
                HttpMethod.Get,
                $"api/terminals/{terminalCode}/printers?locationCode={locationCode}");

        public async Task<ApiResponse<TerminalPrinterAssignmentDto>> SaveTerminalPrintersAsync(TerminalPrinterAssignmentDto dto)
            => await SendAsync<TerminalPrinterAssignmentDto>(
                HttpMethod.Put,
                $"api/terminals/{dto.TerminalCode}/printers",
                dto);

        private async Task<ApiResponse<T>> SendAsync<T>(HttpMethod method, string url, object? body = null)
        {
            try
            {
                using var request = new HttpRequestMessage(method, url);
                if (body != null)
                    request.Content = JsonContent.Create(body);

                var response = await _http.SendAsync(request);
                var raw = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return ApiResponse<T>.Fail("Unauthorized. Please login again.");

                if (string.IsNullOrWhiteSpace(raw))
                    return response.IsSuccessStatusCode
                        ? ApiResponse<T>.Fail("Empty response from server.")
                        : ApiResponse<T>.Fail($"Request failed: {(int)response.StatusCode} {response.ReasonPhrase}");

                var isJson =
                    response.Content.Headers.ContentType?.MediaType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true
                    || raw.TrimStart().StartsWith("{")
                    || raw.TrimStart().StartsWith("[");

                if (!isJson)
                {
                    var preview = BuildErrorPreview(raw);
                    return ApiResponse<T>.Fail(
                        $"Server returned non-JSON response ({(int)response.StatusCode} {response.ReasonPhrase}): {preview}");
                }

                if (!response.IsSuccessStatusCode)
                {
                    var preview = BuildErrorPreview(raw);
                    return ApiResponse<T>.Fail(
                        $"Request failed ({(int)response.StatusCode} {response.ReasonPhrase}): {preview}");
                }

                var parsed = JsonSerializer.Deserialize<ApiResponse<T>>(raw, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (parsed != null)
                    return parsed;

                return ApiResponse<T>.Fail("Invalid server JSON response.");
            }
            catch (Exception ex)
            {
                return ApiResponse<T>.Fail($"Printer API error: {ex.Message}");
            }
        }

        private static string BuildErrorPreview(string raw)
        {
            var cleaned = raw.Replace("\r", "\n");
            var firstMeaningfulLine = cleaned
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
                ?? raw;

            var candidate = firstMeaningfulLine.Length >= 20 ? firstMeaningfulLine : raw.Trim();
            return candidate.Length > 1200 ? candidate[..1200] + "..." : candidate;
        }
    }
}

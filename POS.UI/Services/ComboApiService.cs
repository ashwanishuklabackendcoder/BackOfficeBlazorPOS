using BackOfficeBlazor.Shared.DTOs;
using System.Net.Http.Json;

namespace POS.UI.Services
{
    public class ComboApiService : IComboApiService
    {
        private readonly HttpClient _http;

        public ComboApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ApiResponse<List<ComboGridDto>>> GetGridAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<ApiResponse<List<ComboGridDto>>>("api/combo/grid")
                    ?? ApiResponse<List<ComboGridDto>>.Fail("Null response");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ComboGridDto>>.Fail($"Combo grid load failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ComboMasterDto>> GetByIdAsync(int comboId)
        {
            try
            {
                return await _http.GetFromJsonAsync<ApiResponse<ComboMasterDto>>($"api/combo/{comboId}")
                    ?? ApiResponse<ComboMasterDto>.Fail("Null response");
            }
            catch (Exception ex)
            {
                return ApiResponse<ComboMasterDto>.Fail($"Combo load failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ComboMasterDto>> GetByPartNumberAsync(string comboPartNumber)
        {
            try
            {
                return await _http.GetFromJsonAsync<ApiResponse<ComboMasterDto>>($"api/combo/partnumber/{comboPartNumber}")
                    ?? ApiResponse<ComboMasterDto>.Fail("Null response");
            }
            catch (Exception ex)
            {
                return ApiResponse<ComboMasterDto>.Fail($"Combo lookup failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ComboMasterDto>> CreateAsync(ComboSaveRequestDto request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/combo", request);
                return await response.Content.ReadFromJsonAsync<ApiResponse<ComboMasterDto>>()
                    ?? ApiResponse<ComboMasterDto>.Fail("Null response");
            }
            catch (Exception ex)
            {
                return ApiResponse<ComboMasterDto>.Fail($"Combo create failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ComboMasterDto>> UpdateAsync(int comboId, ComboSaveRequestDto request)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/combo/{comboId}", request);
                return await response.Content.ReadFromJsonAsync<ApiResponse<ComboMasterDto>>()
                    ?? ApiResponse<ComboMasterDto>.Fail("Null response");
            }
            catch (Exception ex)
            {
                return ApiResponse<ComboMasterDto>.Fail($"Combo update failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int comboId)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/combo/{comboId}");
                return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
                    ?? ApiResponse<bool>.Fail("Null response");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Combo delete failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PagedResultDto<ProductSearchDto>>> SearchProductsAsync(string? term, int page, int pageSize)
        {
            try
            {
                return await _http.GetFromJsonAsync<ApiResponse<PagedResultDto<ProductSearchDto>>>(
                    $"api/products/search?term={Uri.EscapeDataString(term ?? string.Empty)}&page={page}&pageSize={pageSize}")
                    ?? ApiResponse<PagedResultDto<ProductSearchDto>>.Fail("Null response");
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResultDto<ProductSearchDto>>.Fail($"Product search failed: {ex.Message}");
            }
        }
    }
}

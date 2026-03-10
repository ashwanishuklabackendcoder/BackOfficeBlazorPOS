using System.Net.Http.Json;
using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly HttpClient _http;

        public SupplierService(HttpClient http)
        {
            _http = http;
        }
        public async Task<List<SupplierDto>> GetAllAsync()
        {
            var result = await _http.GetFromJsonAsync<List<SupplierDto>>(
                "api/supplier/GetAllSuppliers");

            return result ?? new List<SupplierDto>();
        }
        public async Task<ApiResponse<SupplierDto>> GetSupplier(string accountNo)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<ApiResponse<SupplierDto>>
                    ($"api/supplier/GetSuppliers?accountNo={accountNo}");

                return response ?? new ApiResponse<SupplierDto>
                {
                    Success = false,
                    Message = "Null response from server"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SupplierDto>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<SupplierDto>> SaveSupplier(SupplierDto dto)
        {
            try
            {
                var httpResponse = await _http.PostAsJsonAsync("api/supplier/SaveSupplier", dto);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    return new ApiResponse<SupplierDto>
                    {
                        Success = false,
                        Message = $"Error: {httpResponse.StatusCode}"
                    };
                }

                var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<SupplierDto>>();

                return response ?? new ApiResponse<SupplierDto>
                {
                    Success = false,
                    Message = "Deserialization failed"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SupplierDto>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<object>> DeleteSupplier(string accountNo)
        {
            try
            {
                var httpResponse = await _http.DeleteAsync($"api/supplier/DeleteSupplier?accountNo={accountNo}");

                if (!httpResponse.IsSuccessStatusCode)
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Error: {httpResponse.StatusCode}"
                    };
                }

                var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();

                return response ?? new ApiResponse<object>
                {
                    Success = false,
                    Message = "Deserialization failed"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }


        public async Task<List<SupplierDto>> Suggest(string term)
        {
            return await _http.GetFromJsonAsync<List<SupplierDto>>(
                $"api/supplier/SuggestSuppliers?term={term}"
            ) ?? new();
        }

    }
}

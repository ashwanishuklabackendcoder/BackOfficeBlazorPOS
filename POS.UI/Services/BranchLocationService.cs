using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace POS.UI.Services
{
    public class BranchLocationService : IBranchLocationService
    {
        private readonly HttpClient _http;

        public BranchLocationService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<BranchSummaryDto>> GetActiveBranchesAsync()
        {
            var result = await _http.GetFromJsonAsync<List<BranchSummaryDto>>(
                "api/location/Branches");

            return result ?? new List<BranchSummaryDto>();
        }

        public async Task<ApiResponse<BranchDetailDto>> GetBranchAsync(int id)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<ApiResponse<BranchDetailDto>>(
                    $"api/location/Branch/{id}");

                return response ?? new ApiResponse<BranchDetailDto>
                {
                    Success = false,
                    Message = "Null response from server"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<BranchDetailDto>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<BranchDetailDto>> SaveBranchAsync(BranchDetailDto dto)
        {
            try
            {
                var httpResponse = await _http.PostAsJsonAsync("api/location/Branch", dto);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    return new ApiResponse<BranchDetailDto>
                    {
                        Success = false,
                        Message = $"Error: {httpResponse.StatusCode}"
                    };
                }

                var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<BranchDetailDto>>();

                return response ?? new ApiResponse<BranchDetailDto>
                {
                    Success = false,
                    Message = "Deserialization failed"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<BranchDetailDto>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}

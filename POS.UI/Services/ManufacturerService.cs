using BackOfficeBlazor.Shared.DTOs;
using System.Net.Http.Json;

namespace POS.UI.Services
{
    public class ManufacturerService : IManufacturerService
    {
        private readonly HttpClient _http;

        public ManufacturerService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ApiResponse<ManufacturerDto>> GetManufacturer(string code)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<ApiResponse<ManufacturerDto>>
                    ($"api/manufacturer/GetManufacturers?code={code}");

                if (response == null)
                    return new ApiResponse<ManufacturerDto> { Success = false, Message = "Null response" };

                return response;
            }
            catch (Exception ex)
            {
                return new ApiResponse<ManufacturerDto>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
      
        public async Task<ApiResponse<ManufacturerDto>> SaveManufacturer(ManufacturerDto dto)
        {
            try
            {
                var httpResponse = await _http.PostAsJsonAsync("api/manufacturer/SaveManufacturer", dto);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    return new ApiResponse<ManufacturerDto>
                    {
                        Success = false,
                        Message = $"Error: {httpResponse.StatusCode}"
                    };
                }

                var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<ManufacturerDto>>();

                return response ?? new ApiResponse<ManufacturerDto>
                {
                    Success = false,
                    Message = "Deserialization failed"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ManufacturerDto>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<object>> DeleteManufacturer(string code)
        {
            var result = await _http.DeleteAsync($"api/manufacturer/DeleteManufacturer?code={code}");
            return await result.Content.ReadFromJsonAsync<ApiResponse<object>>();
        }
        public async Task<List<string>> SuggestMakes(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                return new();

            try
            {
                return await _http.GetFromJsonAsync<List<string>>(
                    $"api/manufacturer/SuggestMakes?term={term}"
                ) ?? new();
            }
            catch
            {
                return new();
            }
        }


        public async Task<List<ManufacturerDto>> GetAllAsync()
        {
            var result = await _http.GetFromJsonAsync<List<ManufacturerDto>>(
                "api/manufacturer/GetAllManufacturer");

            return result ?? new List<ManufacturerDto>();
        }

        public async Task CreateAsync(string name)
        {
            await _http.PostAsJsonAsync("api/manufacturer", new
            {
                Name = name
            });
        }

        public async Task UpdateAsync(string id, string name)
        {
            await _http.PutAsJsonAsync($"api/manufacturer/{id}", new
            {
                Name = name
            });
        }

    }
}

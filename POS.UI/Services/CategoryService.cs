using BackOfficeBlazor.Shared.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace POS.UI.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _http;

        public CategoryService(HttpClient http)
        {
            _http = http;
        }
        public async Task<List<CategoryDto>> GetAllCategory(string level)
        {
            return await _http.GetFromJsonAsync<List<CategoryDto>>(
                $"api/category/GetAllCategory/{level}"
            ) ?? new();
        }
        //public async Task<List<CategoryDto>> GetAllCategory(string level)
        //{
        //    var response = await _http.GetAsync($"api/category/GetAllCategory?level={level}");
        //    var raw = await response.Content.ReadAsStringAsync();

        //    Console.WriteLine(raw); // DEBUG

        //    response.EnsureSuccessStatusCode();

        //    return JsonSerializer.Deserialize<List<CategoryDto>>(raw,
        //        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        //}

        public async Task<ApiResponse<CategoryDto>> GetCategory(string code)
        {
            return await _http.GetFromJsonAsync<ApiResponse<CategoryDto>>
                ($"api/category/GetCategories?code={code}")
                ?? new ApiResponse<CategoryDto> { Success = false };
        }

        public async Task<ApiResponse<CategoryDto>> SaveCategory(CategoryDto model)
        {
            var result = await _http.PostAsJsonAsync("api/category/SaveCategory", model);
            return await result.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>();
        }

        public async Task<ApiResponse<object>> DeleteCategory(string code)
        {
            var result = await _http.DeleteAsync($"api/category/DeleteCategory/{code}");
            return await result.Content.ReadFromJsonAsync<ApiResponse<object>>();
        }
        public async Task<List<CategoryDto>> Suggest(string type, string query)
        {
            var result = await _http.GetFromJsonAsync<ApiResponse<List<CategoryDto>>>(
                $"api/category/suggest?type={type}&query={query}");

            return result?.Data ?? new();
        }
    }
}

using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllCategory(string level);

        Task<ApiResponse<CategoryDto>> GetCategory(string code);
        Task<ApiResponse<CategoryDto>> SaveCategory(CategoryDto model);
        Task<ApiResponse<object>> DeleteCategory(string code);
        Task<List<CategoryDto>> Suggest(string type, string query);
    }
}

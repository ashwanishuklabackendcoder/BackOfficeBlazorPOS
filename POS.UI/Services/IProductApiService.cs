using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public interface IProductApiService
    {
        Task<ApiResponse<ProductDto>> GetProduct(string partNumber);
        Task<ApiResponse<ProductDto>> SaveProduct(ProductDto dto);
        Task<ApiResponse<bool>> SaveGroupProduct(GroupProductDto group);
        Task<ApiResponse<List<ProductDto>>> GetProductsAsync(ProductFilterDto filter);


    }
}

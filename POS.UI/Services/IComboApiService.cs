using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public interface IComboApiService
    {
        Task<ApiResponse<List<ComboGridDto>>> GetGridAsync();
        Task<ApiResponse<ComboMasterDto>> GetByIdAsync(int comboId);
        Task<ApiResponse<ComboMasterDto>> GetByPartNumberAsync(string comboPartNumber);
        Task<ApiResponse<ComboMasterDto>> CreateAsync(ComboSaveRequestDto request);
        Task<ApiResponse<ComboMasterDto>> UpdateAsync(int comboId, ComboSaveRequestDto request);
        Task<ApiResponse<bool>> DeleteAsync(int comboId);
        Task<ApiResponse<PagedResultDto<ProductSearchDto>>> SearchProductsAsync(string? term, int page, int pageSize);
    }
}

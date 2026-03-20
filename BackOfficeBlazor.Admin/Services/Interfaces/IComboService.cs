using BackOfficeBlazor.Shared.DTOs;

namespace BackOfficeBlazor.Admin.Services.Interfaces
{
    public interface IComboService
    {
        Task<ApiResponse<ComboMasterDto>> CreateCombo(ComboSaveRequestDto request);
        Task<ApiResponse<ComboMasterDto>> UpdateCombo(ComboSaveRequestDto request);
        Task<ApiResponse<List<ComboGridDto>>> GetComboGrid();
        Task<ApiResponse<ComboMasterDto>> GetComboById(int comboId);
        Task<ApiResponse<bool>> SoftDeleteCombo(int comboId);
        Task<ApiResponse<ComboMasterDto>> GetActiveComboByPartNumber(string comboPartNumber);
        Task<ApiResponse<PagedResultDto<ProductSearchDto>>> SearchProducts(string? term, int page, int pageSize);
    }
}

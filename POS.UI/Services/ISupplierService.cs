using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public interface ISupplierService
    {
        Task<List<SupplierDto>> GetAllAsync();
        
        Task<ApiResponse<SupplierDto>> GetSupplier(string accountNo);
        Task<ApiResponse<SupplierDto>> SaveSupplier(SupplierDto dto);
        Task<ApiResponse<object>> DeleteSupplier(string accountNo);
        Task<List<SupplierDto>> Suggest(string term);

    }
}

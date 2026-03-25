using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Shared.DTOs;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface IComboRepository
    {
        Task<int> GenerateNextComboPartNumberAsync();
        Task<bool> ComboPartNumberExistsAsync(string comboPartNumber, int? excludeComboId = null);
        Task<ComboMaster?> GetByIdAsync(int comboId);
        Task<ComboMaster?> GetByIdWithDetailsAsync(int comboId);
        Task<ComboMaster?> GetActiveByPartNumberAsync(string comboPartNumber);
        Task<List<ComboGridDto>> GetGridAsync();
        Task<PagedResultDto<ProductSearchDto>> SearchProductsAsync(string? term, int page, int pageSize);
        Task<List<ProductItem>> GetProductsByPartNumbersAsync(List<string> partNumbers);
        Task AddAsync(ComboMaster entity);
        Task AddDetailAsync(ComboDetail entity);
        Task RemoveDetailAsync(ComboDetail entity);
        Task SaveChangesAsync();
    }
}

using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public interface IManufacturerService
    {
        Task<List<ManufacturerDto>> GetAllAsync();
        Task CreateAsync(string name);
        Task UpdateAsync(string code, string name);
        Task<ApiResponse<ManufacturerDto>> GetManufacturer(string code);
        Task<ApiResponse<ManufacturerDto>> SaveManufacturer(ManufacturerDto dto);
        Task<ApiResponse<object>> DeleteManufacturer(string code);
        Task<List<string>> SuggestMakes(string term);

    }
}

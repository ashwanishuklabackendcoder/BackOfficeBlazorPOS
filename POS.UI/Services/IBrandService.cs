using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public interface IBrandService
    {
        Task<List<ManufacturerDto>> GetAllAsync();
        Task CreateAsync(string name);
        Task UpdateAsync(int id, string name);
    }
}

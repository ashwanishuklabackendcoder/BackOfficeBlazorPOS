using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public interface ILocationService
    {
        Task<List<LocationDto>> GetAllAsync();
    }
}

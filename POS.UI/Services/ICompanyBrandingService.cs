using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public interface ICompanyBrandingService : IDisposable
    {
        event Action? Changed;
        CompanyBrandingDto? Current { get; }
        Task<CompanyBrandingDto?> GetAsync(bool forceRefresh = false);
    }
}

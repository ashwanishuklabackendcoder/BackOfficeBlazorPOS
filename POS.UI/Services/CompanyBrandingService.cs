using System.Net.Http.Json;
using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public class CompanyBrandingService : ICompanyBrandingService
    {
        private readonly HttpClient _http;
        private readonly ISysOptionsService _sysOptions;
        public event Action? Changed;
        public CompanyBrandingDto? Current { get; private set; }

        public CompanyBrandingService(HttpClient http, ISysOptionsService sysOptions)
        {
            _http = http;
            _sysOptions = sysOptions;
            _sysOptions.Changed += HandleSysOptionsChanged;
        }

        public async Task<CompanyBrandingDto?> GetAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && Current != null)
                return Current;

            try
            {
                var response = await _http.GetFromJsonAsync<ApiResponse<CompanyBrandingDto>>("api/company-branding")
                    ?? ApiResponse<CompanyBrandingDto>.Fail("Null response");

                if (response.Success && response.Data != null)
                {
                    Current = response.Data;
                    Changed?.Invoke();
                    return Current;
                }
            }
            catch
            {
                // Branding is a best-effort UI concern. Fall back instead of breaking the layout.
            }

            Current = new CompanyBrandingDto();
            Changed?.Invoke();
            return Current;
        }

        private void HandleSysOptionsChanged(SysOptionsDto? options)
        {
            Current = new CompanyBrandingDto
            {
                CompanyName = options?.CompanyName ?? string.Empty,
                CompanyLogoUrl = options?.CompanyLogoUrl ?? string.Empty
            };
            Changed?.Invoke();
        }

        public void Dispose()
        {
            _sysOptions.Changed -= HandleSysOptionsChanged;
        }
    }
}

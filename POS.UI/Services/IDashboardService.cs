using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public interface IDashboardService
    {
        Task<ApiResponse<DashboardResponseDto>> GetDashboardAsync(DashboardRequestDto request);
    }
}

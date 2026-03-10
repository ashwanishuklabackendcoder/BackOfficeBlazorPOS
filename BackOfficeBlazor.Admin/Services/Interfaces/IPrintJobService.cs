using BackOfficeBlazor.Shared.DTOs;

namespace BackOfficeBlazor.Admin.Services.Interfaces
{
    public interface IPrintJobService
    {
        Task<ApiResponse<PrintJobDto>> EnqueueAsync(PrintJobRequestDto request);
        Task<ApiResponse<bool>> UpdateStatusAsync(PrintJobStatusUpdateDto update);
    }
}

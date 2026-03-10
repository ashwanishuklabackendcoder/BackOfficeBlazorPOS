using BackOfficeBlazor.Shared.DTOs;

namespace BackOfficeBlazor.Admin.Services.Interfaces
{
    public interface IPrinterConfigService
    {
        Task<ApiResponse<List<PrinterListItemDto>>> GetByLocationAsync(string locationCode);
        Task<ApiResponse<PrinterConfigDto>> GetAsync(int id);
        Task<ApiResponse<PrinterConfigDto>> SaveAsync(PrinterConfigDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<PrintJobDto>> EnqueueTestPrintAsync(int id, string terminalCode);
    }
}

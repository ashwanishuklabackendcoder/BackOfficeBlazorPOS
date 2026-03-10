using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public interface IPrinterApiService
    {
        Task<ApiResponse<List<PrinterListItemDto>>> GetPrintersAsync(string locationCode);
        Task<ApiResponse<PrinterConfigDto>> GetPrinterAsync(int id);
        Task<ApiResponse<PrinterConfigDto>> CreatePrinterAsync(PrinterConfigDto dto);
        Task<ApiResponse<PrinterConfigDto>> UpdatePrinterAsync(PrinterConfigDto dto);
        Task<ApiResponse<bool>> DeletePrinterAsync(int id);
        Task<ApiResponse<PrintJobDto>> TestPrinterAsync(int id, string terminalCode);
        Task<ApiResponse<TerminalPrinterCatalogDto>> GetTerminalPrintersAsync(string terminalCode, string locationCode);
        Task<ApiResponse<TerminalPrinterAssignmentDto>> SaveTerminalPrintersAsync(TerminalPrinterAssignmentDto dto);
    }
}

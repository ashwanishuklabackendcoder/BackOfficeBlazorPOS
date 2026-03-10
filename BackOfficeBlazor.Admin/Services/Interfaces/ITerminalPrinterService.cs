using BackOfficeBlazor.Shared.DTOs;

namespace BackOfficeBlazor.Admin.Services.Interfaces
{
    public interface ITerminalPrinterService
    {
        Task<ApiResponse<TerminalPrinterCatalogDto>> GetPrintersForTerminalAsync(string terminalCode, string locationCode);
        Task<ApiResponse<TerminalPrinterAssignmentDto>> SaveAssignmentAsync(TerminalPrinterAssignmentDto dto);
    }
}

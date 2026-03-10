using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    public class TerminalPrinterService : ITerminalPrinterService
    {
        private readonly ITerminalOptionRepository _terminalOptions;
        private readonly IPrinterConfigRepository _printers;

        public TerminalPrinterService(
            ITerminalOptionRepository terminalOptions,
            IPrinterConfigRepository printers)
        {
            _terminalOptions = terminalOptions;
            _printers = printers;
        }

        public async Task<ApiResponse<TerminalPrinterCatalogDto>> GetPrintersForTerminalAsync(string terminalCode, string locationCode)
        {
            terminalCode = terminalCode?.Trim() ?? "";
            if (!string.IsNullOrWhiteSpace(terminalCode) && terminalCode.Length > 1)
                return ApiResponse<TerminalPrinterCatalogDto>.Fail("TerminalCode must be 1 character");

            var printers = await _printers.GetByLocationAsync(locationCode);
            var assignment = await _terminalOptions.GetByTerminalCodeAsync(terminalCode);

            var dto = new TerminalPrinterCatalogDto
            {
                TerminalCode = terminalCode,
                LocationCode = locationCode,
                Printers = printers.Select(x => new PrinterListItemDto
                {
                    Id = x.Id,
                    LocationCode = x.LocationCode,
                    PrinterName = x.PrinterName,
                    Type = x.Type,
                    Mode = x.Mode,
                    IpAddress = x.IpAddress
                }).ToList(),
                Assignment = new TerminalPrinterAssignmentDto
                {
                    TerminalCode = terminalCode,
                    LocationCode = locationCode,
                    ReceiptPrinterId = assignment?.ReceiptPrinterId
                        ?? printers.FirstOrDefault(x => x.Type == (int)PrinterType.Receipt)?.Id
                        ?? 0,
                    A4PrinterId = assignment?.A4PrinterId
                        ?? printers.FirstOrDefault(x => x.Type == (int)PrinterType.A4)?.Id,
                    LabelPrinterId = assignment?.LabelPrinterId
                        ?? printers.FirstOrDefault(x => x.Type == (int)PrinterType.Label)?.Id
                }
            };

            return ApiResponse<TerminalPrinterCatalogDto>.Ok(dto);
        }

        public async Task<ApiResponse<TerminalPrinterAssignmentDto>> SaveAssignmentAsync(TerminalPrinterAssignmentDto dto)
        {
            dto.TerminalCode = dto.TerminalCode?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(dto.TerminalCode))
                return ApiResponse<TerminalPrinterAssignmentDto>.Fail("TerminalCode is required");
            if (dto.TerminalCode.Length > 1)
                return ApiResponse<TerminalPrinterAssignmentDto>.Fail("TerminalCode must be 1 character");

            await _terminalOptions.EnsureTerminalExistsAsync(dto.TerminalCode, dto.LocationCode);

            var printers = await _printers.GetByLocationAsync(dto.LocationCode);
            if (!printers.Any(x => x.Id == dto.ReceiptPrinterId && x.Type == (int)PrinterType.Receipt))
                return ApiResponse<TerminalPrinterAssignmentDto>.Fail("Receipt printer is invalid for this location");

            if (dto.A4PrinterId.HasValue && !printers.Any(x => x.Id == dto.A4PrinterId.Value && x.Type == (int)PrinterType.A4))
                return ApiResponse<TerminalPrinterAssignmentDto>.Fail("A4 printer is invalid for this location");

            if (dto.LabelPrinterId.HasValue && !printers.Any(x => x.Id == dto.LabelPrinterId.Value && x.Type == (int)PrinterType.Label))
                return ApiResponse<TerminalPrinterAssignmentDto>.Fail("Label printer is invalid for this location");

            var existing = await _terminalOptions.GetByTerminalCodeAsync(dto.TerminalCode);
            if (existing == null)
            {
                var available = await _terminalOptions.GetTerminalCodesAsync(dto.LocationCode);
                var suffix = available.Count > 0
                    ? $" Available terminal codes for location {dto.LocationCode}: {string.Join(", ", available)}."
                    : $" No terminal codes found for location {dto.LocationCode} in TerminalOptions.";
                return ApiResponse<TerminalPrinterAssignmentDto>.Fail(
                    $"Terminal '{dto.TerminalCode}' not found in TerminalOptions. Configure the terminal first, then assign printers.{suffix}");
            }

            var entity = new TerminalOption
            {
                TerminalCode = dto.TerminalCode,
                ReceiptPrinterId = dto.ReceiptPrinterId,
                A4PrinterId = dto.A4PrinterId,
                LabelPrinterId = dto.LabelPrinterId
            };

            await _terminalOptions.UpsertAsync(entity);
            await _terminalOptions.SaveChangesAsync();
            return ApiResponse<TerminalPrinterAssignmentDto>.Ok(dto, "Assignment saved");
        }
    }
}

using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    public class PrintJobService : IPrintJobService
    {
        private readonly IPrintJobRepository _jobs;
        private readonly IPrinterConfigRepository _printers;

        public PrintJobService(IPrintJobRepository jobs, IPrinterConfigRepository printers)
        {
            _jobs = jobs;
            _printers = printers;
        }

        public async Task<ApiResponse<PrintJobDto>> EnqueueAsync(PrintJobRequestDto request)
        {
            var printer = await _printers.GetByIdAsync(request.PrinterConfigId);
            if (printer == null)
                return ApiResponse<PrintJobDto>.Fail("Printer not found");

            var entity = new PrintJob
            {
                TerminalCode = request.TerminalCode,
                PrinterConfigId = request.PrinterConfigId,
                JobType = request.JobType,
                Payload = request.Payload,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _jobs.AddAsync(entity);
            await _jobs.SaveChangesAsync();

            return ApiResponse<PrintJobDto>.Ok(ToDto(entity), "Print job queued");
        }

        public async Task<ApiResponse<bool>> UpdateStatusAsync(PrintJobStatusUpdateDto update)
        {
            var entity = await _jobs.GetByIdAsync(update.JobId);
            if (entity == null)
                return ApiResponse<bool>.Fail("Print job not found");

            entity.Status = update.Status;
            entity.ProcessedAt = update.Status is "Completed" or "Failed" ? DateTime.UtcNow : null;
            await _jobs.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Status updated");
        }

        private static PrintJobDto ToDto(PrintJob x)
        {
            return new PrintJobDto
            {
                Id = x.Id,
                TerminalCode = x.TerminalCode,
                PrinterConfigId = x.PrinterConfigId,
                JobType = x.JobType,
                Payload = x.Payload,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                ProcessedAt = x.ProcessedAt
            };
        }
    }
}

using System.Text.Json;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    public class PrinterConfigService : IPrinterConfigService
    {
        private readonly IPrinterConfigRepository _repo;
        private readonly IPrintJobRepository _jobs;
        private readonly IPrintJobService _printJobs;
        private readonly ISecretEncryptionService _encryption;
        private readonly IPrintAgentPresenceService _presence;
        private readonly IEscPosBuilder _escPos;
        private readonly IZplBuilder _zpl;

        public PrinterConfigService(
            IPrinterConfigRepository repo,
            IPrintJobRepository jobs,
            IPrintJobService printJobs,
            ISecretEncryptionService encryption,
            IPrintAgentPresenceService presence,
            IEscPosBuilder escPos,
            IZplBuilder zpl)
        {
            _repo = repo;
            _jobs = jobs;
            _printJobs = printJobs;
            _encryption = encryption;
            _presence = presence;
            _escPos = escPos;
            _zpl = zpl;
        }

        public async Task<ApiResponse<List<PrinterListItemDto>>> GetByLocationAsync(string locationCode)
        {
            var entities = await _repo.GetByLocationAsync(locationCode);
            var lastPrints = await _jobs.GetLastCompletedAtByPrinterIdsAsync(entities.Select(x => x.Id));
            var isOnline = _presence.IsLocationOnline(locationCode);

            var rows = entities.Select(x => new PrinterListItemDto
            {
                Id = x.Id,
                LocationCode = x.LocationCode,
                PrinterName = x.PrinterName,
                Type = x.Type,
                Mode = x.Mode,
                IpAddress = x.IpAddress,
                IsOnline = isOnline,
                LastPrintedAt = lastPrints.TryGetValue(x.Id, out var last) ? last : null
            }).ToList();

            return ApiResponse<List<PrinterListItemDto>>.Ok(rows);
        }

        public async Task<ApiResponse<PrinterConfigDto>> GetAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                return ApiResponse<PrinterConfigDto>.Fail("Printer not found");

            var dto = ToDto(entity);
            dto.SharePassword = SafeDecrypt(entity.SharePassword);
            return ApiResponse<PrinterConfigDto>.Ok(dto);
        }

        public async Task<ApiResponse<PrinterConfigDto>> SaveAsync(PrinterConfigDto dto)
        {
            var validation = Validate(dto);
            if (!validation.IsValid)
                return ApiResponse<PrinterConfigDto>.Fail(validation.Message);

            var exists = await _repo.ExistsByNameAsync(dto.LocationCode, dto.PrinterName, dto.Id == 0 ? null : dto.Id);
            if (exists)
                return ApiResponse<PrinterConfigDto>.Fail("Printer name already exists in this location");

            if (dto.Id == 0)
            {
                var entity = FromDto(dto);
                entity.SharePassword = _encryption.Encrypt(dto.SharePassword ?? "");
                entity.DateCreated = DateTime.UtcNow;
                await _repo.AddAsync(entity);
                await _repo.SaveChangesAsync();
                dto.Id = entity.Id;
                return ApiResponse<PrinterConfigDto>.Ok(dto, "Printer created");
            }

            var existing = await _repo.GetByIdAsync(dto.Id);
            if (existing == null)
                return ApiResponse<PrinterConfigDto>.Fail("Printer not found");

            existing.LocationCode = dto.LocationCode;
            existing.PrinterName = dto.PrinterName;
            existing.Description = dto.Description;
            existing.Mode = dto.Mode;
            existing.Type = dto.Type;
            existing.LabelFormat = dto.LabelFormat;
            existing.IpAddress = dto.IpAddress;
            existing.TcpPort = dto.TcpPort;
            existing.LocalPortName = dto.LocalPortName;
            existing.Shared = dto.Shared;
            existing.ShareHostAddress = dto.ShareHostAddress;
            existing.ShareName = dto.ShareName;
            existing.ShareUsername = dto.ShareUsername;
            existing.SharePassword = _encryption.Encrypt(dto.SharePassword ?? "");
            existing.ReceiptPrintLogo = dto.ReceiptPrintLogo;
            existing.ReceiptLogoSize = dto.ReceiptLogoSize;
            existing.Filename = dto.Filename;
            existing.FileExtension = dto.FileExtension;
            existing.FileEnsureUnique = dto.FileEnsureUnique;
            existing.EmailDestination = dto.EmailDestination;
            existing.DateUpdated = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);
            await _repo.SaveChangesAsync();
            return ApiResponse<PrinterConfigDto>.Ok(dto, "Printer updated");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                return ApiResponse<bool>.Fail("Printer not found");

            await _repo.DeleteAsync(entity);
            await _repo.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Printer deleted");
        }

        public async Task<ApiResponse<PrintJobDto>> EnqueueTestPrintAsync(int id, string terminalCode)
        {
            var printer = await _repo.GetByIdAsync(id);
            if (printer == null)
                return ApiResponse<PrintJobDto>.Fail("Printer not found");

            var payload = printer.Type switch
            {
                (int)PrinterType.Receipt => _escPos.BuildTestReceipt(),
                (int)PrinterType.Label => _zpl.BuildTestLabel(),
                _ => JsonSerializer.Serialize(new
                {
                    title = "Cloud POS Test A4",
                    body = $"Generated at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"
                })
            };

            var jobType = printer.Type switch
            {
                (int)PrinterType.Receipt => "Receipt",
                (int)PrinterType.Label => "Label",
                _ => "A4"
            };

            return await _printJobs.EnqueueAsync(new PrintJobRequestDto
            {
                TerminalCode = terminalCode,
                PrinterConfigId = id,
                JobType = jobType,
                Payload = payload
            });
        }

        private PrinterValidationResultDto Validate(PrinterConfigDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.LocationCode))
                return Invalid("Location is required");
            if (string.IsNullOrWhiteSpace(dto.PrinterName))
                return Invalid("PrinterName is required");
            if (string.IsNullOrWhiteSpace(dto.Description))
                return Invalid("Description is required");

            if (dto.Type == (int)PrinterType.A4 && dto.Mode != (int)PrinterMode.Windows)
                return Invalid("A4 printers must use Windows mode");
            if (dto.Type == (int)PrinterType.Label && dto.LabelFormat != (int)LabelFormat.Zpl)
                return Invalid("Label printers must use ZPL format");
            if (dto.Type == (int)PrinterType.Receipt && dto.Mode == (int)PrinterMode.Tcp && dto.LabelFormat == (int)LabelFormat.Zpl)
                return Invalid("Receipt TCP printers must use ESC/POS payload");

            if (dto.Mode == (int)PrinterMode.Tcp)
            {
                if (string.IsNullOrWhiteSpace(dto.IpAddress))
                    return Invalid("IpAddress is required for TCP mode");
                if (dto.TcpPort <= 0)
                    return Invalid("TcpPort is required for TCP mode");
            }

            if (dto.Mode == (int)PrinterMode.Windows && string.IsNullOrWhiteSpace(dto.LocalPortName))
                return Invalid("LocalPortName is required for Windows mode");

            if (dto.Mode == (int)PrinterMode.File)
            {
                if (string.IsNullOrWhiteSpace(dto.Filename))
                    return Invalid("Filename is required for File mode");
                if (string.IsNullOrWhiteSpace(dto.FileExtension))
                    return Invalid("FileExtension is required for File mode");
            }

            if (dto.Mode == (int)PrinterMode.Email && string.IsNullOrWhiteSpace(dto.EmailDestination))
                return Invalid("EmailDestination is required for Email mode");

            return new PrinterValidationResultDto { IsValid = true };
        }

        private static PrinterValidationResultDto Invalid(string message)
            => new() { IsValid = false, Message = message };

        private PrinterConfigDto ToDto(PrinterConfig x)
        {
            return new PrinterConfigDto
            {
                Id = x.Id,
                LocationCode = x.LocationCode,
                PrinterName = x.PrinterName,
                Description = x.Description,
                Mode = x.Mode,
                Type = x.Type,
                LabelFormat = x.LabelFormat,
                IpAddress = x.IpAddress,
                TcpPort = x.TcpPort,
                LocalPortName = x.LocalPortName,
                Shared = x.Shared,
                ShareHostAddress = x.ShareHostAddress,
                ShareName = x.ShareName,
                ShareUsername = x.ShareUsername,
                SharePassword = x.SharePassword,
                ReceiptPrintLogo = x.ReceiptPrintLogo,
                ReceiptLogoSize = x.ReceiptLogoSize,
                Filename = x.Filename,
                FileExtension = x.FileExtension,
                FileEnsureUnique = x.FileEnsureUnique,
                EmailDestination = x.EmailDestination
            };
        }

        private static PrinterConfig FromDto(PrinterConfigDto x)
        {
            return new PrinterConfig
            {
                Id = x.Id,
                LocationCode = x.LocationCode,
                PrinterName = x.PrinterName,
                Description = x.Description,
                Mode = x.Mode,
                Type = x.Type,
                LabelFormat = x.LabelFormat,
                IpAddress = x.IpAddress ?? "",
                TcpPort = x.TcpPort,
                LocalPortName = x.LocalPortName ?? "",
                Shared = x.Shared,
                ShareHostAddress = x.ShareHostAddress ?? "",
                ShareName = x.ShareName ?? "",
                ShareUsername = x.ShareUsername ?? "",
                SharePassword = x.SharePassword ?? "",
                ReceiptPrintLogo = x.ReceiptPrintLogo,
                ReceiptLogoSize = x.ReceiptLogoSize,
                Filename = x.Filename ?? "",
                FileExtension = x.FileExtension ?? "",
                FileEnsureUnique = x.FileEnsureUnique,
                EmailDestination = x.EmailDestination ?? ""
            };
        }

        private string SafeDecrypt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            try
            {
                return _encryption.Decrypt(value);
            }
            catch
            {
                return "";
            }
        }
    }
}

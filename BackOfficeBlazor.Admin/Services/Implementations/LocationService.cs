using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    public class LocationService : ILocationService
    {
        private readonly IlocationRepositry _repo;
        private readonly ISettingsRepository _settingsRepo;

        private const string SettingsJsonVersion = "v1";

        public LocationService(IlocationRepositry repo, ISettingsRepository settingsRepo)
        {
            _repo = repo;
            _settingsRepo = settingsRepo;
        }

        public async Task<List<LocationDto>> GetAllLocation()
        {
            var suppliers = await _repo.GetAllLocation();

            return suppliers.Select(x => new LocationDto
            {
                Code = x.Code,
                Name = x.Name
            }).ToList();
        }

        public async Task<List<BranchSummaryDto>> GetActiveBranchesAsync()
        {
            var branches = await _repo.GetActiveLocations();

            return branches.Select(x => new BranchSummaryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted,
                KeyLocation = x.KeyLocation
            }).ToList();
        }

        public async Task<ApiResponse<BranchDetailDto>> GetBranchAsync(int id)
        {
            try
            {
                var entity = await _repo.GetByIdAsync(id);
                if (entity == null)
                    return ApiResponse<BranchDetailDto>.Fail("Branch not found");

                var settings = await _settingsRepo.GetByBranchIdAsync(id);
                var dto = ToDetailDto(entity, settings);

                return ApiResponse<BranchDetailDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<BranchDetailDto>.Fail(ex.Message);
            }
        }

        public async Task<ApiResponse<BranchDetailDto>> SaveBranchAsync(BranchDetailDto dto)
        {
            try
            {
                var validation = Validate(dto);
                if (!string.IsNullOrEmpty(validation))
                    return ApiResponse<BranchDetailDto>.Fail(validation);

                if (await _repo.CodeExistsAsync(dto.Code.Trim().ToUpper(), dto.Id == 0 ? null : dto.Id))
                    return ApiResponse<BranchDetailDto>.Fail("Code must be unique.");

                Location? entity = dto.Id == 0 ? null : await _repo.GetByIdAsync(dto.Id);

                if (entity != null && entity.IsDeleted)
                    return ApiResponse<BranchDetailDto>.Fail("This branch is deleted and cannot be edited.");

                if (entity == null)
                {
                    entity = new Location
                    {
                        DateCreated = DateTime.UtcNow
                    };

                    entity = FromDto(dto, entity);
                    await _repo.AddAsync(entity);
                }
                else
                {
                    entity = FromDto(dto, entity);
                    entity.DateUpdated = DateTime.UtcNow;
                    await _repo.UpdateAsync(entity);
                }

                await _repo.SaveChangesAsync();

                await SaveSettingsAsync(entity.Id, dto.Settings);

                var savedSettings = await _settingsRepo.GetByBranchIdAsync(entity.Id);
                var resultDto = ToDetailDto(entity, savedSettings);

                return ApiResponse<BranchDetailDto>.Ok(resultDto, "Branch saved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<BranchDetailDto>.Fail($"Save failed: {ex.Message}");
            }
        }

        private BranchDetailDto ToDetailDto(Location x, Setting? settings)
        {
            return new BranchDetailDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                CompanyNumber = x.CompanyNumber,
                Address1 = x.Address1,
                Address2 = x.Address2,
                Address3 = x.Address3,
                Address4 = x.Address4,
                Postcode = x.Postcode,
                GeneralEmailAddress = x.GeneralEmailAddress,
                MainTelephone = x.MainTelephone,
                AccountEmail = x.AccountEmail,
                AccountName = x.AccountName,
                AdminEmail = x.AdminEmail,
                AdminName = x.AdminName,
                StoreWebsiteURL = x.StoreWebsiteURL,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted,
                KeyLocation = x.KeyLocation,
                DateCreated = x.DateCreated,
                DateUpdated = x.DateUpdated,
                Settings = MapSettings(settings)
            };
        }

        private Location FromDto(BranchDetailDto dto, Location entity)
        {
            entity.Code = dto.Code.Trim().ToUpper();
            entity.Name = dto.Name.Trim();
            entity.CompanyNumber = dto.CompanyNumber.Trim();
            entity.Address1 = TrimOrEmpty(dto.Address1);
            entity.Address2 = TrimOrEmpty(dto.Address2);
            entity.Address3 = TrimOrEmpty(dto.Address3);
            entity.Address4 = TrimOrEmpty(dto.Address4);
            entity.Postcode = dto.Postcode.Trim();
            entity.GeneralEmailAddress = TrimOrEmpty(dto.GeneralEmailAddress);
            entity.MainTelephone = TrimOrEmpty(dto.MainTelephone);
            entity.AccountEmail = TrimOrEmpty(dto.AccountEmail);
            entity.AccountName = TrimOrEmpty(dto.AccountName);
            entity.AdminEmail = TrimOrEmpty(dto.AdminEmail);
            entity.AdminName = TrimOrEmpty(dto.AdminName);
            entity.StoreWebsiteURL = TrimOrEmpty(dto.StoreWebsiteURL);
            entity.IsActive = dto.IsActive;
            entity.IsDeleted = dto.IsDeleted;
            entity.KeyLocation = dto.KeyLocation;

            return entity;
        }

        private static string TrimOrEmpty(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value.Trim();
        }

        private static string? Validate(BranchDetailDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code))
                return "Code is required.";
            if (dto.Code.Trim().Length > 2)
                return "Code must be 2 characters or fewer.";

            if (string.IsNullOrWhiteSpace(dto.Name))
                return "Name is required.";

            if (string.IsNullOrWhiteSpace(dto.CompanyNumber))
                return "Company Number is required.";

            if (string.IsNullOrWhiteSpace(dto.Postcode))
                return "Postcode is required.";

            if (!string.IsNullOrEmpty(dto.MainTelephone) && dto.MainTelephone.Length > 30)
                return "Telephone number must be 30 characters or fewer.";

            if (!IsValidEmail(dto.GeneralEmailAddress))
                return "General Email Address is invalid.";

            if (!IsValidEmail(dto.AccountEmail))
                return "Account Email is invalid.";

            if (!IsValidEmail(dto.AdminEmail))
                return "Admin Email is invalid.";

            if (!IsValidEmail(dto.Settings?.EcommerceContactEmail))
                return "ECommerce Contact Email is invalid.";

            if (!IsValidEmail(dto.Settings?.WorkshopEmail))
                return "Workshop Email is invalid.";

            return null;
        }

        private static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true;

            return Regex.IsMatch(email.Trim(),
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.CultureInvariant);
        }

        private BranchSettingsDto MapSettings(Setting? settings)
        {
            if (settings == null || string.IsNullOrWhiteSpace(settings.SimConfig))
                return new BranchSettingsDto { OpeningHours = DefaultOpeningHours() };

            try
            {
                var envelope = JsonSerializer.Deserialize<BranchSettingsEnvelope>(settings.SimConfig);
                var dto = envelope?.Settings ?? new BranchSettingsDto();
                dto.OpeningHours ??= DefaultOpeningHours();
                if (dto.OpeningHours.Count == 0)
                    dto.OpeningHours = DefaultOpeningHours();
                return dto;
            }
            catch
            {
                return new BranchSettingsDto { OpeningHours = DefaultOpeningHours() };
            }
        }

        private async Task SaveSettingsAsync(int branchId, BranchSettingsDto? settings)
        {
            var dto = settings ?? new BranchSettingsDto();
            dto.OpeningHours ??= DefaultOpeningHours();
            if (dto.OpeningHours.Count == 0)
                dto.OpeningHours = DefaultOpeningHours();

            var envelope = new BranchSettingsEnvelope
            {
                Version = SettingsJsonVersion,
                Settings = dto
            };
            var json = JsonSerializer.Serialize(envelope);

            var existing = await _settingsRepo.GetByBranchIdAsync(branchId);
            if (existing == null)
            {
                await _settingsRepo.AddAsync(new Setting
                {
                    BranchId = branchId,
                    SimConfig = json,
                    MadisonDealerId = string.Empty
                });
            }
            else
            {
                existing.SimConfig = json;
                await _settingsRepo.UpdateAsync(existing);
            }

            await _settingsRepo.SaveChangesAsync();
        }

        private static List<OpeningHourDto> DefaultOpeningHours()
        {
            return new List<OpeningHourDto>
            {
                new OpeningHourDto { Day = "Monday", OpenTime = new TimeOnly(9, 0), CloseTime = new TimeOnly(17, 0), IsClosed = false },
                new OpeningHourDto { Day = "Tuesday", OpenTime = new TimeOnly(9, 0), CloseTime = new TimeOnly(17, 0), IsClosed = false },
                new OpeningHourDto { Day = "Wednesday", OpenTime = new TimeOnly(9, 0), CloseTime = new TimeOnly(17, 0), IsClosed = false },
                new OpeningHourDto { Day = "Thursday", OpenTime = new TimeOnly(9, 0), CloseTime = new TimeOnly(17, 0), IsClosed = false },
                new OpeningHourDto { Day = "Friday", OpenTime = new TimeOnly(9, 0), CloseTime = new TimeOnly(17, 0), IsClosed = false },
                new OpeningHourDto { Day = "Saturday", OpenTime = new TimeOnly(10, 0), CloseTime = new TimeOnly(16, 0), IsClosed = false },
                new OpeningHourDto { Day = "Sunday", OpenTime = null, CloseTime = null, IsClosed = true }
            };
        }

        private sealed class BranchSettingsEnvelope
        {
            public string Version { get; set; } = SettingsJsonVersion;
            public BranchSettingsDto Settings { get; set; } = new();
        }
    }
}

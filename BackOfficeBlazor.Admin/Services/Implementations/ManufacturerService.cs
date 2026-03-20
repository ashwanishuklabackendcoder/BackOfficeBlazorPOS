using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Implementations;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    public class ManufacturerService : IManufacturerService
    {
        private readonly IManufacturerRepository _repo;

        public ManufacturerService(IManufacturerRepository repo)
        {
            _repo = repo;
        }
        public async Task<List<ManufacturerDto>> GetAll()
        {
            var manufacturers = await _repo.GetAllManufacturer();

            return manufacturers.Select(x => new ManufacturerDto
            {
                Code = x.Code,
                Name = x.Name
            }).ToList();
        }

        public async Task<ApiResponse<ManufacturerDto>> GetAsync(string code)
        {
            var entity = await _repo.GetByCodeAsync(code);
            if (entity == null)
            {
                return ApiResponse<ManufacturerDto>.Fail("Manufacturer not found");
            }

            return ApiResponse<ManufacturerDto>.Ok(ToDto(entity));
        }

        public async Task<ApiResponse<ManufacturerDto>> SaveAsync(ManufacturerDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return ApiResponse<ManufacturerDto>.Fail("Manufacturer Name is required");
                }

                if (string.IsNullOrWhiteSpace(dto.Code))
                {
                    var length = SequenceHelper.GetMaxLength<Manufacturer>("Code", 4);
                    var last = await _repo.GetLastCodeAsync();
                    dto.Code = SequenceHelper.GenerateNext(last, length);
                }

                dto.Code = dto.Code?.Trim().ToUpperInvariant();

                var duplicateByName = await _repo.GetByNameAsync(dto.Name);
                if (duplicateByName != null &&
                    !string.Equals(duplicateByName.Code, dto.Code, StringComparison.OrdinalIgnoreCase))
                {
                    return ApiResponse<ManufacturerDto>.Fail("Manufacturer already exists");
                }

                var entity = await _repo.GetByCodeAsync(dto.Code);

                if (entity == null)
                {
                    // INSERT
                    entity = new Manufacturer
                    {
                        Code = dto.Code.Trim().ToUpper(),
                        Name = dto.Name.Trim()
                    };

                    await _repo.AddAsync(entity);
                    await _repo.SaveChangesAsync();

                    return ApiResponse<ManufacturerDto>.Ok(dto, "Manufacturer added successfully");
                }
                else
                {
                    // UPDATE
                    entity.Name = dto.Name.Trim();

                    await _repo.UpdateAsync(entity);
                    await _repo.SaveChangesAsync();

                    return ApiResponse<ManufacturerDto>.Ok(dto, "Manufacturer updated successfully");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<ManufacturerDto>.Fail($"Failed to save: {ex.Message}");
            }
        }

        public async Task<ApiResponse<object>> DeleteAsync(string code)
        {
            var entity = await _repo.GetByCodeAsync(code);
            if (entity == null)
            {
                return ApiResponse<object>.Fail("Manufacturer not found");
            }

            await _repo.DeleteAsync(entity);
            await _repo.SaveChangesAsync();

            return ApiResponse<object>.Ok(null, "Manufacturer deleted successfully");
        }

        private ManufacturerDto ToDto(Manufacturer x) =>
            new ManufacturerDto { Code = x.Code, Name = x.Name };

        public async Task<List<string>> SuggestMakes(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                return new();

            return await _repo.SuggestMakesAsync(term);
        }


    }
}

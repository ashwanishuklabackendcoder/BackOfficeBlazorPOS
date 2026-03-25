using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;

namespace BackOfficeBlazor.Admin.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo)
    {
        _repo = repo;
    }
    public async Task<List<CategoryDto>> GetAllCategory(string level)
    {
        return await _repo.GetAllCategory(level);
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var categories = await _repo.GetAllAsync();

        return categories.Select(c => new CategoryDto
        {
            Code = c.Code,
            Name = c.Name
        })
        .ToList();
    }
    public async Task<CategoryDto?> GetAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var entity = await _repo.GetByCodeAsync(code);
        if (entity == null) return null;

        return new CategoryDto
        {
            Code = entity.Code,
            Name = entity.Name,
            A = entity.A,
            B = entity.B,
            C = entity.C,
            Major = entity.Major
        };
    }
    public async Task<CategoryDto> CreateAsync(CategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code))
        {
            var last = await _repo.GetLastCodeAsync();
            dto.Code = SequenceHelper.GenerateNextFiveDigitCode(last);
        }

        dto.Code = dto.Code?.Trim().ToUpperInvariant();

        var entity = new Category
        {
            Code = dto.Code,
            Name = dto.Name,
            A = dto.A,
            B = dto.B,
            C = dto.C,
            IsDeleted = dto.IsDeleted,
            Major = dto.Major,
            DateCreated = dto.DateCreated
        };

        await _repo.AddAsync(entity);

        dto.Code = entity.Code;
        return dto;
    }
  
        public async Task<CategoryDto> UpdateAsync(CategoryDto dto)
        {
            dto.Code = dto.Code?.Trim().ToUpperInvariant();
            var entity = await _repo.GetByCodeAsync(dto.Code);

            if (entity == null)
                return null; // or throw exception

            entity.Name = dto.Name;
            entity.A = dto.A;
            entity.B = dto.B;
            entity.C = dto.C;
            entity.Major = dto.Major;

            await _repo.UpdateAsync(entity);

            return dto;
        }


    public async Task<bool> DeleteAsync(string code)
    {
        var entity = await _repo.GetByCodeAsync(code); // Correct retrieval
        if (entity == null)
            return false;

        await _repo.DeleteAsync(entity.Code); 
        return true;
    }
    public async Task<List<CategoryDto>> SuggestAsync(string type, string query)
    {
        try
        {
            var list = await _repo.SuggestAsync(type, query);

            return list.Select(x => new CategoryDto
            {
                Code = x.Code,
                Name = x.Name,
                A = x.A,
                B = x.B,
                C = x.C
            })
            .ToList();
        }
        catch (Exception ex)
        {
            // OPTIONAL: add logger if you are using ILogger<CategoryService>
            // _logger.LogError(ex, "Error while fetching category suggestions.");

            // Return empty list safely so API does not crash
            return new List<CategoryDto>();
        }
    }

}

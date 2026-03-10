using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Interfaces
{
    public interface IManufacturerService
    {
        Task<List<ManufacturerDto>> GetAll();

        Task<ApiResponse<ManufacturerDto>> GetAsync(string code);
        Task<ApiResponse<ManufacturerDto>> SaveAsync(ManufacturerDto dto);
        Task<ApiResponse<object>> DeleteAsync(string code);
        Task<List<string>> SuggestMakes(string term);

    }
}

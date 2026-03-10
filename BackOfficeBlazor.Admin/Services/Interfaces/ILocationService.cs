using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Interfaces
{
    public interface ILocationService
    {
        Task<List<LocationDto>> GetAllLocation();
        Task<List<BranchSummaryDto>> GetActiveBranchesAsync();
        Task<ApiResponse<BranchDetailDto>> GetBranchAsync(int id);
        Task<ApiResponse<BranchDetailDto>> SaveBranchAsync(BranchDetailDto dto);
    }
}

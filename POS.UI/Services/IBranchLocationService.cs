using BackOfficeBlazor.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POS.UI.Services
{
    public interface IBranchLocationService
    {
        Task<List<BranchSummaryDto>> GetActiveBranchesAsync();
        Task<ApiResponse<BranchDetailDto>> GetBranchAsync(int id);
        Task<ApiResponse<BranchDetailDto>> SaveBranchAsync(BranchDetailDto dto);
    }
}

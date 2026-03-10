using BackOfficeBlazor.Shared.DTOs;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Interfaces
{
    public interface ISysOptionService
    {
        Task<ApiResponse<SysOptionsDto>> GetAsync();
        Task<ApiResponse<SysOptionsDto>> SaveAsync(SysOptionsDto dto);
    }
}

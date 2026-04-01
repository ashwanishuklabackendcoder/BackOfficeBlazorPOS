using BackOfficeBlazor.Shared.DTOs;
using System.Threading.Tasks;

namespace POS.UI.Services
{
    public interface ISysOptionsService
    {
        event Action<SysOptionsDto?>? Changed;
        Task<ApiResponse<SysOptionsDto>> GetAsync();
        Task<ApiResponse<SysOptionsDto>> SaveAsync(SysOptionsDto dto);
        SysOptionsDto? Current { get; }
    }
}

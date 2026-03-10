using BackOfficeBlazor.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POS.UI.Services
{
    public interface ILayawayService
    {
        Task<ApiResponse<int>> CreateAsync(LayawayCreateDto request);
        Task<ApiResponse<List<LayawaySummaryDto>>> GetActiveAsync(string? customerAccNo);
        Task<ApiResponse<LayawayDetailDto>> GetAsync(int layawayNo);
        Task<ApiResponse<bool>> AddLineAsync(int layawayNo, LayawayLineDto line);
        Task<ApiResponse<bool>> UpdateLineAsync(int layawayNo, LayawayLineUpdateDto update);
        Task<ApiResponse<bool>> ReverseAsync(int layawayNo);
        Task<ApiResponse<string>> SellAsync(LayawaySellRequestDto request);
    }
}

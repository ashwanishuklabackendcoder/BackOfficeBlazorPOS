using BackOfficeBlazor.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Interfaces
{
    public interface ILayawayService
    {
        Task<int> CreateAsync(LayawayCreateDto request);
        Task<List<LayawaySummaryDto>> GetActiveAsync(string? customerAccNo);
        Task<LayawayDetailDto?> GetAsync(int layawayNo);
        Task AddLineAsync(int layawayNo, LayawayLineDto line);
        Task UpdateLineQtyAsync(int layawayNo, LayawayLineUpdateDto update);
        Task ReverseAsync(int layawayNo);
        Task<string> SellAsync(LayawaySellRequestDto request);
    }
}

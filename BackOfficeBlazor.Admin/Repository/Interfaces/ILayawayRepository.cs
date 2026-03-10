using BackOfficeBlazor.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface ILayawayRepository
    {
        Task<int> CreateAsync(LayawayCreateDto request);
        Task<List<LayawaySummaryDto>> GetActiveAsync(string? customerAccNo);
        Task<LayawayDetailDto?> GetAsync(int layawayNo);
        Task AddLineAsync(int layawayNo, LayawayLineDto line);
        Task UpdateLineQtyAsync(int layawayNo, LayawayLineUpdateDto update);
        Task ReverseAsync(int layawayNo);
        Task<List<LayawayLineDto>> GetLinesAsync(int layawayNo);
        Task MarkStatusAsync(int layawayNo, int layawayType, bool reserved);
    }
}

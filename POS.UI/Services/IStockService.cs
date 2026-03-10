using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public interface IStockService
    {
        Task<StockInputResultDto> SaveAsync(StockInputDto dto);
        Task<int> GetCurrentStockAsync(string partNumber);
        Task<List<StockNumberDto>> GetAvailableStockNumbersAsync(string partNumber, string location);

        Task<List<StockHistoryDto>> GetHistoryAsync(string partNumber);
        Task<List<ProductStockLevelDto>> GetStockLevelsAsync(string partNumber);
    }

}

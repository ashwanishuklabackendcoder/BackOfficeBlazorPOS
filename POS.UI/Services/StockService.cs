using BackOfficeBlazor.Shared.DTOs;
using System.Net.Http.Json;

namespace POS.UI.Services
{
    public class StockService : IStockService
    {
        private readonly HttpClient _http;

        public StockService(HttpClient http)
        {
            _http = http;
        }

        public async Task<StockInputResultDto> SaveAsync(StockInputDto dto)
        {
            var res = await _http.PostAsJsonAsync("api/stock/input", dto); 

            if (!res.IsSuccessStatusCode)
                return new StockInputResultDto
                {
                    Success = false,
                    Message = await res.Content.ReadAsStringAsync()
                };

            return await res.Content.ReadFromJsonAsync<StockInputResultDto>()
                   ?? new StockInputResultDto { Success = false };
        }

        public async Task<int> GetCurrentStockAsync(string partNumber)
        {
            return await _http.GetFromJsonAsync<int>(
                $"api/stock/current/{partNumber}");
        }
        public async Task<List<StockNumberDto>> GetAvailableStockNumbersAsync( string partNumber, string location)
        {
            return await _http.GetFromJsonAsync<List<StockNumberDto>>(
                $"api/stock/available-serials/{partNumber}/{location}")
                ?? new List<StockNumberDto>();
        }

        public async Task<List<StockHistoryDto>> GetHistoryAsync(string partNumber)
        {
            return await _http.GetFromJsonAsync<List<StockHistoryDto>>(
                $"api/stock/history/{partNumber}")
                ?? new();
        }
        public async Task<List<ProductStockLevelDto>> GetStockLevelsAsync(string partNumber)
        {
            return await _http.GetFromJsonAsync<List<ProductStockLevelDto>>(
                $"api/stock/levels/{partNumber}"
            ) ?? new();
        }
    }

}

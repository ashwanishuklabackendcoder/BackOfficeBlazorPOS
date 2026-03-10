using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Interfaces
{
    public interface IStockInputService
    {
        Task<StockInputResultDto> SaveAsync(StockInputDto dto);
        Task<int> GetCurrentStockAsync(string partNumber);
        Task<List<StockHistoryDto>> GetStockHistoryAsync(string partNumber);
        //Task<List<StockNumberDto>> GetAvailableSerialsAsync(string partNumber,string location);
        Task<List<StockNumberDto>> GetAvailableStockNumbersAsync(string partNumber, string location);

        Task<List<ProductStockLevelDto>> GetStockLevelsAsync(string partNumber);

    }

}

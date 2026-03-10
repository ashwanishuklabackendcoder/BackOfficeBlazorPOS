using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface IProductStockRepository
    {
        Task InsertAsync(Stock stock);
        Task<List<StockHistoryDto>> GetHistoryAsync(string partNumber);
        //Task<List<StockNumberDto>> GetSerialsAsync(string partNumber,string location);
        Task<List<StockNumberDto>> GetAvailableStockNumbersAsync(string partNumber, string location);

    }

}

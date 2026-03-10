using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface IStockLevelRepository
    {
        Task EnsureExistsAsync(string partNumber);
        Task IncrementAsync(string partNumber, string locationCode, int quantity);
        Task<int> GetTotalStockAsync(string partNumber);
        Task<List<ProductStockLevelDto>> GetStockLevelsAsync(string partNumber);
      

    }

}

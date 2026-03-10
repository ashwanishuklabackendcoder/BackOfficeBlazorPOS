using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class StockLevelRepository: IStockLevelRepository
    {
        private readonly BackOfficeAdminContext _context;

        public StockLevelRepository(BackOfficeAdminContext context)
        {
            _context = context;
        }
        public async Task EnsureExistsAsync(string partNumber)
        {
            var exists = await _context._ProductsStockLevels
                .AnyAsync(x => x.PartNumber == partNumber);

            if (!exists)
            {
                _context._ProductsStockLevels.Add(new StockLevels
                {
                    PartNumber = partNumber,
                    DateCreated = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }
        }
        public async Task<int> GetTotalStockAsync(string partNumber)
        {
            var row = await _context._ProductsStockLevels
                .FirstOrDefaultAsync(x => x.PartNumber == partNumber);

            if (row == null)
                return 0;

            int total = 0;

            for (int i = 1; i <= 30; i++)
            {
                var prop = typeof(StockLevels).GetProperty($"L{i:D2}");
                if (prop != null)
                    total += (int)prop.GetValue(row)!;
            }

            return total;
        }

        public async Task IncrementAsync(string partNumber, string locationCode, int quantity)
        {
            var level = await _context._ProductsStockLevels
                .FirstAsync(x => x.PartNumber == partNumber);

            var prop = typeof(StockLevels)
                .GetProperty($"L{locationCode}");

            if (prop == null)
                throw new Exception("Invalid location code");

            var current = (int)prop.GetValue(level)!;
            prop.SetValue(level, current + quantity);

            level.DateUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        public async Task<List<ProductStockLevelDto>> GetStockLevelsAsync(string partNumber)
        {
            var row = await _context._ProductsStockLevels
                .FirstOrDefaultAsync(x => x.PartNumber == partNumber);

            var result = new List<ProductStockLevelDto>();

            if (row == null)
                return result;

            for (int i = 1; i <= 30; i++)
            {
                var prop = typeof(StockLevels).GetProperty($"L{i:D2}");
                var qty = (int)(prop?.GetValue(row) ?? 0);

                result.Add(new ProductStockLevelDto
                {
                    LocationCode = i.ToString("D2"),
                    Quantity = qty
                });
            }

            return result;
        }
  

    }
}

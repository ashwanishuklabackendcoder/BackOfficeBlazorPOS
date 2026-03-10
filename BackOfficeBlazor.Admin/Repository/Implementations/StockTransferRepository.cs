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
    public class StockTransferRepository : IStockTransferRepository
    {
        private readonly BackOfficeAdminContext _db;

        public StockTransferRepository(BackOfficeAdminContext db)
        {
            _db = db;
        }

        public async Task ApplyTransferAsync(StockTransferInputDto dto)
        {
            var levels = await _db._ProductsStockLevels
                .FirstOrDefaultAsync(x => x.PartNumber == dto.PartNumber);

            if (levels == null)
                throw new Exception("Stock levels not found for part");

            var fromProp = typeof(StockLevels)
                .GetProperty($"L{dto.FromLocation}");

            var toProp = typeof(StockLevels)
                .GetProperty($"L{dto.ToLocation}");

            if (fromProp == null || toProp == null)
                throw new Exception("Invalid location code");

            var fromQty = (int)(fromProp.GetValue(levels) ?? 0);

            if (fromQty < dto.Quantity)
                throw new Exception("Insufficient stock at source location");

            // UPDATE LEVELS
            fromProp.SetValue(levels, fromQty - dto.Quantity);

            var toQty = (int)(toProp.GetValue(levels) ?? 0);
            toProp.SetValue(levels, toQty + dto.Quantity);

            var totalStock =
                Enumerable.Range(1, 30)
                .Select(i =>
                {
                    var p = typeof(ProductStockLevelDto)
                        .GetProperty($"L{i:D2}");
                    return (int)(p?.GetValue(levels) ?? 0);
                })
                .Sum();

            // INSERT INTO ProductStockMovement (AUDIT)
            _db.ProductStockMovement.Add(new ProductStockMovement
            {
                DateAndTime = DateTime.UtcNow,
                PartNo = dto.PartNumber,
                StockQty = dto.Quantity,
                Notes = dto.Notes,
                SalesCode = dto.SalesCode,
                FromLocation = dto.FromLocation,
                ToLocation = dto.ToLocation,
                TotalCurrentStock = totalStock
            });

            await _db.SaveChangesAsync();
        }
    }

}

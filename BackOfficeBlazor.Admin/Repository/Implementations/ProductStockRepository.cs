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
    public class ProductStockRepository:IProductStockRepository
    {
        private readonly BackOfficeAdminContext _context;

        public ProductStockRepository(BackOfficeAdminContext context)
        {
            _context = context;
        }

        public async Task InsertAsync(Stock stock)
        {
            _context._ProductsStock.Add(stock);
            await _context.SaveChangesAsync();
        }
        public async Task<List<StockHistoryDto>> GetHistoryAsync(string partNumber)
        {
            return await _context._ProductsStock
                .Where(x => x.PartNumber == partNumber)
                .OrderByDescending(x => x.DateCreated)
                .Select(x => new StockHistoryDto
                {
                    PartNumber = x.PartNumber,
                    LocationCode = x.LocationCode,
                    Quantity = x.Quantity,
                    Cost = x.Cost,
                    DateCreated = x.DateCreated
                })
                .ToListAsync();
        }
        public async Task<List<StockNumberDto>> GetAvailableStockNumbersAsync(string partNumber, string location)
        {
            return await _context._ProductsStock
                .Where(x => x.PartNumber == partNumber
                         && x.LocationCode == location
                         && x.IsAvailable == true)
                .Select(x => new StockNumberDto
                {
                    StockNumber = x.StockNumber
                })
                .ToListAsync();
        }



        //public async Task<List<StockNumberDto>> GetSerialsAsync(string partNumber,string location)
        //{
        //    return await _context._ProductsStock
        //        .Where(x => x.PartNumber == partNumber && x.LocationCode==location)
        //        .OrderByDescending(x => x.DateCreated)
        //        .Select(x => new StockNumberDto
        //        {
        //            StockNumber = x.StockNumber,

        //        })
        //        .ToListAsync();
        //}

    }
}

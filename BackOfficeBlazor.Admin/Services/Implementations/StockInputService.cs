using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Implementations;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    public class StockInputService : IStockInputService
    {
        private readonly IProductRepository _productRepo;
        private readonly IProductStockRepository _stockRepo;
        private readonly IStockLevelRepository _levelRepo;
        private readonly BackOfficeAdminContext _context;

        public StockInputService(
            IProductRepository productRepo,
            IProductStockRepository stockRepo,
            IStockLevelRepository levelRepo,
            BackOfficeAdminContext context)
        {
            _productRepo = productRepo;
            _stockRepo = stockRepo;
            _levelRepo = levelRepo;
            _context = context;
        }

        public async Task<StockInputResultDto> SaveAsync(StockInputDto dto)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var product = await _productRepo.GetByPartNumberAsync(dto.PartNumber);
                if (product == null)
                    return Fail("Product not found");

                await _levelRepo.EnsureExistsAsync(dto.PartNumber);

                if (!product.Major)
                {
                    await _stockRepo.InsertAsync(new Stock
                    {
                        PartNumber = dto.PartNumber,
                        LocationCode = dto.LocationCode,
                        Quantity = dto.Quantity,
                        Cost = dto.CostEach,
                        StockNumber = "0000000000",
                        CustomerAccNo = "000000",
                        SerialNumber = "N/A",
                        SupplierCode = dto.SupplierCode,
                        InvoiceNumber = dto.InvoiceNumber,
                        PurchaseOrderNo = dto.PurchaseOrderNo,
                        PrintLabelOption = dto.PrintLabel ? 1 : 0,
                        IsAvailable = true,
                        IsPrinted = false,
                        IsCollected = false,
                        DateCreated = DateTime.UtcNow,
                        StaffCode = dto.StaffCode
                    });
                }
                else
                {
                    for (int i = 0; i < dto.Quantity; i++)
                    {
                        await _stockRepo.InsertAsync(new Stock
                        {
                            PartNumber = dto.PartNumber,
                            LocationCode = dto.LocationCode,
                            Quantity = 1,
                            CustomerAccNo = "000000",
                            SerialNumber = "N/A",
                            StockNumber = GenerateStockNumber(10),
                            Cost = dto.CostEach,
                            SupplierCode = dto.SupplierCode,
                            InvoiceNumber = dto.InvoiceNumber,
                            PurchaseOrderNo = dto.PurchaseOrderNo,
                            PrintLabelOption = dto.PrintLabel ? 1 : 0,
                            IsAvailable = true,
                            IsPrinted = false,
                            IsCollected = false,
                            DateCreated = DateTime.UtcNow,
                            StaffCode = dto.StaffCode
                        });
                    }
                }

                await _levelRepo.IncrementAsync(
                    dto.PartNumber,
                    dto.LocationCode,
                    dto.Quantity);

                await tx.CommitAsync();

                return new StockInputResultDto
                {
                    Success = true,
                    QuantityAdded = dto.Quantity,
                    Message = "Stock added successfully"
                };
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return Fail(ex.Message);
            }
        }
        public static string GenerateStockNumber(int length = 10)
        {
            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);

            var digits = bytes
                .Select(b => (b % 10).ToString())
                .ToArray();

            return string.Concat(digits);
        }

        public async Task<int> GetCurrentStockAsync(string partNumber)
        {
            return await _levelRepo.GetTotalStockAsync(partNumber);
        }

        public async Task<List<StockHistoryDto>> GetStockHistoryAsync(string partNumber)
        {
            return await _stockRepo.GetHistoryAsync(partNumber);
        }
        public async Task<List<StockNumberDto>> GetAvailableStockNumbersAsync(string partNumber,string location)
        {
            return await _stockRepo.GetAvailableStockNumbersAsync(
                partNumber, location);
        }

        //public async Task<List<StockNumberDto>> GetAvailableSerialsAsync(string partNumber,string location)
        //{
        //    return await _stockRepo.GetSerialsAsync(partNumber,location);
        //}
        public Task<List<ProductStockLevelDto>> GetStockLevelsAsync(string partNumber)
        {
            return _levelRepo.GetStockLevelsAsync(partNumber);
        }
        private StockInputResultDto Fail(string msg) =>
            new() { Success = false, Message = msg };
    }

}

using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface IProductRepository
    {
        Task<ProductItem?> GetByPartNumberAsync(string partNumber);
        Task<ProductItem?> GetByMfrPartNumberAsync(string MfrpartNumber);
        Task<ProductItem?> GetByBarcodeNumberAsync(string Barcode);
        Task<List<ProductItem>> GetAllAsync(ProductFilterDto filter);

        Task<string?> GetLastPartNumberAsync();
        Task AddAsync(ProductItem entity);
        Task UpdateAsync(ProductItem entity);
        Task SaveChangesAsync();
    }
}

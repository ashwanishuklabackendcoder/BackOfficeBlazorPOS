using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Interfaces
{
    public interface ISupplierService
    {
        Task<List<SupplierDto>> GetAll();
        Task<ApiResponse<SupplierDto>> GetAsync(string accountNo);
        Task<ApiResponse<SupplierDto>> SaveAsync(SupplierDto dto);
        Task<ApiResponse<object>> DeleteAsync(string accountNo);
        Task<List<SupplierDto>> SuggestSuppliers(string query);
    }
}

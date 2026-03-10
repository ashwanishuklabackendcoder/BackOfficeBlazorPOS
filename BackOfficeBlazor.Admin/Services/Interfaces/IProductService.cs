using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Interfaces
{
    public interface IProductService
    {
        Task<ApiResponse<ProductDto>> GetAsync(string partNumber);

        Task<ApiResponse<ProductDto>> GetMFRPartNo(string MfrpartNumber);
        Task<ApiResponse<ProductDto>> GetBarcode(string Barcode);
        Task<ApiResponse<GroupProductDto>> GetGroupAsync(string partNumber);
        Task<ApiResponse<ProductDto>> SaveAsync(ProductDto dto);
        Task<ApiResponse<List<ProductDto>>> GetAllAsync(ProductFilterDto filter);

        Task<ApiResponse<bool>> SaveGroupProduct(GroupProductDto dto);
    }
}

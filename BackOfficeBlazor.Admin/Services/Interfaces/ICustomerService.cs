using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<ApiResponse<CustomerDto>> GetAsync(string accNo);
        Task<ApiResponse<List<CustomerDto>>> GetAllAsync();
        Task<ApiResponse<List<CustomerDto>>> SearchAsync(CustomerSearchRequestDto request);
        Task<ApiResponse<CustomerDto>> SaveAsync(CustomerDto dto);
        Task<ApiResponse<object>> DeleteAsync(string accNo);
    }
}

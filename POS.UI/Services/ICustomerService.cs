using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
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

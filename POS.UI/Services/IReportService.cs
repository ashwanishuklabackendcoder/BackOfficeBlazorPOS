using BackOfficeBlazor.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POS.UI.Services
{
    public interface IReportService
    {
        Task<ApiResponse<List<CustomerSalesReturnLineDto>>> GetCustomerSalesReturnsAsync(
            CustomerSalesReturnReportRequestDto request);
    }
}

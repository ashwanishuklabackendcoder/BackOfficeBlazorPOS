using BackOfficeBlazor.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Interfaces
{
    public interface IReportsService
    {
        Task<List<CustomerSalesReturnLineDto>> GetCustomerSalesReturnsAsync(CustomerSalesReturnReportRequestDto request);
    }
}

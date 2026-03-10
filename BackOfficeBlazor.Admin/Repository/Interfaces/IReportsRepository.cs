using BackOfficeBlazor.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface IReportsRepository
    {
        Task<List<CustomerSalesReturnLineDto>> GetCustomerSalesReturnsAsync(CustomerSalesReturnReportRequestDto request);
    }
}

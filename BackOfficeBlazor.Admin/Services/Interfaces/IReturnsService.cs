using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Interfaces
{
    public interface IReturnsService
    {
        Task ProcessReturnAsync(ReturnProcessDto dto);
        Task<List<ReturnInvoiceLookupDto>> GetInvoicesAsync(DateTime? fromDate, DateTime? toDate, string? customerAccNo);
        Task<List<PosSaleLineDto>> GetInvoiceLinesAsync(string invoiceNo);
        Task<PosReceiptDto?> GetReceiptAsync(string invoiceNo);

    }

}

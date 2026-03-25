using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface ISalesRepository
    {
        Task<string> GenerateNextInvoiceNumberAsync(string location, string terminal);

        Task InsertFtt05Async(string invoiceNo,
                               PosSaleRequestDto header,
                               PosSaleLineDto line);

        Task InsertFtt11Async(string invoiceNo,
                               PosSaleRequestDto header);

        Task UpdateStockAfterSaleAsync(PosSaleLineDto line, string location);
    }

}

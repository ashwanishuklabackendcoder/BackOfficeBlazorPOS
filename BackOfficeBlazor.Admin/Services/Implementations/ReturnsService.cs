using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    public class ReturnsService : IReturnsService
    {
        private readonly IReturnsRepository _repo;

        public ReturnsService(IReturnsRepository repo)
        {
            _repo = repo;
        }

        public async Task ProcessReturnAsync(ReturnProcessDto dto)
        {
            await _repo.ProcessReturnAsync(dto);
        }

        public async Task<List<ReturnInvoiceLookupDto>> GetInvoicesAsync(DateTime? fromDate, DateTime? toDate, string? customerAccNo)
        {
            return await _repo.GetInvoicesAsync(fromDate, toDate, customerAccNo);
        }

        public async Task<List<PosSaleLineDto>> GetInvoiceLinesAsync(string invoiceNo)
        {
            return await _repo.GetInvoiceLinesAsync(invoiceNo);
        }

        public async Task<PosReceiptDto?> GetReceiptAsync(string invoiceNo)
        {
            return await _repo.GetReceiptAsync(invoiceNo);
        }

    }

}

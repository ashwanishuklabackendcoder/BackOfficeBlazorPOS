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
    public class StockTransferService : IStockTransferService
    {
        private readonly IStockTransferRepository _repo;

        public StockTransferService(IStockTransferRepository repo)
        {
            _repo = repo;
        }

        public Task ApplyTransferAsync(StockTransferInputDto dto)
            => _repo.ApplyTransferAsync(dto);
    }

}

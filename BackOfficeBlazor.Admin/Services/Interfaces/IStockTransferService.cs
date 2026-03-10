using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Interfaces
{
    public interface IStockTransferService
    {
        Task ApplyTransferAsync(StockTransferInputDto dto);
    }

}

using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public interface IStockTransferUiService
    {
        Task<ApiResponse<bool>> ApplyTransferAsync(StockTransferInputDto dto);
    }

}

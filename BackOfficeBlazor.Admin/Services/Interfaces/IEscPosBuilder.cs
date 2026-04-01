using BackOfficeBlazor.Shared.DTOs;

namespace BackOfficeBlazor.Admin.Services.Interfaces
{
    public interface IEscPosBuilder
    {
        string BuildTestReceipt();
        string BuildSaleReceipt(string invoiceNo, PosSaleRequestDto sale);
        string BuildSaleInvoiceHtml(string invoiceNo, PosSaleRequestDto sale, SysOptionsDto? sysOptions);
    }
}

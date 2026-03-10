using BackOfficeBlazor.Admin.Entities;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface IPrinterConfigRepository
    {
        Task<List<PrinterConfig>> GetByLocationAsync(string locationCode);
        Task<PrinterConfig?> GetByIdAsync(int id);
        Task AddAsync(PrinterConfig entity);
        Task UpdateAsync(PrinterConfig entity);
        Task DeleteAsync(PrinterConfig entity);
        Task<bool> ExistsByNameAsync(string locationCode, string printerName, int? excludingId = null);
        Task SaveChangesAsync();
    }
}

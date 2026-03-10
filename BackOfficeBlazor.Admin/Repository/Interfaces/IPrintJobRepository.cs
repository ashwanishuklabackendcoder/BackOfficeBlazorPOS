using BackOfficeBlazor.Admin.Entities;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface IPrintJobRepository
    {
        Task AddAsync(PrintJob job);
        Task<PrintJob?> GetByIdAsync(int id);
        Task<Dictionary<int, DateTime?>> GetLastCompletedAtByPrinterIdsAsync(IEnumerable<int> printerConfigIds);
        Task SaveChangesAsync();
    }
}

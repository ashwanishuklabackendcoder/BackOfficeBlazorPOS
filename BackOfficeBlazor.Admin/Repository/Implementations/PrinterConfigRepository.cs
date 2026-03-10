using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class PrinterConfigRepository : IPrinterConfigRepository
    {
        private readonly BackOfficeAdminContext _context;

        public PrinterConfigRepository(BackOfficeAdminContext context)
        {
            _context = context;
        }

        public Task<List<PrinterConfig>> GetByLocationAsync(string locationCode)
            => _context.PrinterConfigs
                .Where(x => x.LocationCode == locationCode)
                .OrderBy(x => x.PrinterName)
                .ToListAsync();

        public Task<PrinterConfig?> GetByIdAsync(int id)
            => _context.PrinterConfigs.FirstOrDefaultAsync(x => x.Id == id);

        public Task AddAsync(PrinterConfig entity)
            => _context.PrinterConfigs.AddAsync(entity).AsTask();

        public Task UpdateAsync(PrinterConfig entity)
        {
            _context.PrinterConfigs.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(PrinterConfig entity)
        {
            _context.PrinterConfigs.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByNameAsync(string locationCode, string printerName, int? excludingId = null)
            => _context.PrinterConfigs.AnyAsync(x =>
                x.LocationCode == locationCode &&
                x.PrinterName == printerName &&
                (!excludingId.HasValue || x.Id != excludingId.Value));

        public Task SaveChangesAsync()
            => _context.SaveChangesAsync();
    }
}

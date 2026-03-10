using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class PrintJobRepository : IPrintJobRepository
    {
        private readonly BackOfficeAdminContext _context;

        public PrintJobRepository(BackOfficeAdminContext context)
        {
            _context = context;
        }

        public Task AddAsync(PrintJob job)
            => _context.PrintJobs.AddAsync(job).AsTask();

        public Task<PrintJob?> GetByIdAsync(int id)
            => _context.PrintJobs.FirstOrDefaultAsync(x => x.Id == id);

        public async Task<Dictionary<int, DateTime?>> GetLastCompletedAtByPrinterIdsAsync(IEnumerable<int> printerConfigIds)
        {
            var ids = printerConfigIds.Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<int, DateTime?>();

            var rows = await _context.PrintJobs
                .Where(x => ids.Contains(x.PrinterConfigId) && x.Status == "Completed")
                .GroupBy(x => x.PrinterConfigId)
                .Select(g => new { PrinterConfigId = g.Key, Last = g.Max(x => x.ProcessedAt) })
                .ToListAsync();

            return rows.ToDictionary(x => x.PrinterConfigId, x => x.Last);
        }

        public Task SaveChangesAsync()
            => _context.SaveChangesAsync();
    }
}

using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class locationRepositry : IlocationRepositry
    {
        private readonly BackOfficeAdminContext _context;
        
        public locationRepositry(BackOfficeAdminContext context)
        {
            _context = context;
        }
        public async Task<List<Location>> GetAllLocation()
        {
            return await _context.Set<Location>()
                                 .OrderBy(x => x.Code) // optional
                                 .ToListAsync();
        }

        public async Task<List<Location>> GetActiveLocations()
        {
            return await _context.Set<Location>()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.Code)
                .ToListAsync();
        }

        public async Task<Location?> GetByIdAsync(int id)
        {
            return await _context.Set<Location>()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Location?> GetByCodeAsync(string code)
        {
            return await _context.Set<Location>()
                .FirstOrDefaultAsync(x => x.Code == code);
        }

        public async Task AddAsync(Location location)
        {
            await _context.Set<Location>().AddAsync(location);
        }

        public async Task UpdateAsync(Location location)
        {
            _context.Set<Location>().Update(location);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CodeExistsAsync(string code, int? excludeId)
        {
            var query = _context.Set<Location>().AsQueryable()
                .Where(x => x.Code == code);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}

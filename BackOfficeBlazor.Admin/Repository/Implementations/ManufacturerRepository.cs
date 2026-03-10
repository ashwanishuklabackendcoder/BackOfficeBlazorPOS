using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class ManufacturerRepository : IManufacturerRepository
    {
        private readonly BackOfficeAdminContext _context;

        public ManufacturerRepository(BackOfficeAdminContext context)
        {
            _context = context;
        }
        public async Task<List<Manufacturer>> GetAllManufacturer()
        {
            return await _context.Set<Manufacturer>()
                                 .OrderBy(x => x.Name) // optional
                                 .ToListAsync();
        }

        public async Task<Manufacturer?> GetByCodeAsync(string code)
        {
            return await _context.Set<Manufacturer>()
                .FirstOrDefaultAsync(x => x.Code == code);
        }

        public async Task<Manufacturer?> GetByNameAsync(string name)
        {
            var normalized = name.Trim();

            return await _context.Set<Manufacturer>()
                .FirstOrDefaultAsync(x => x.Name != null && x.Name.ToLower() == normalized.ToLower());
        }

        public async Task AddAsync(Manufacturer entity)
        {
            await _context.Set<Manufacturer>().AddAsync(entity);
        }

        public async Task UpdateAsync(Manufacturer entity)
        {
            _context.Set<Manufacturer>().Update(entity);
        }

        public async Task DeleteAsync(Manufacturer entity)
        {
            _context.Set<Manufacturer>().Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<List<string>> SuggestMakesAsync(string term)
        {
            return await _context._Makes
                .Where(m => m.Name.Contains(term))
                .Select(m => m.Name)
                .Distinct()
                .OrderBy(m => m)
                .Take(10)
                .ToListAsync();
        }


    }
}

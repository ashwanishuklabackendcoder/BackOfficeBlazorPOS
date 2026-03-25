using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly BackOfficeAdminContext _context;

        public SupplierRepository(BackOfficeAdminContext context)
        {
            _context = context;
        }
        public async Task<List<Supplier>> GetAllSupplier()
        {
            return await _context.Set<Supplier>()
                                 .OrderBy(x => x.Name) // optional
                                 .ToListAsync();
        }
        public async Task<Supplier?> GetByAccountNoAsync(string accNo)
        {
            return await _context.Set<Supplier>()
                .FirstOrDefaultAsync(x => x.AccountNo == accNo && !x.IsDeleted);
        }

        public async Task AddAsync(Supplier supplier)
        {
            await _context.Set<Supplier>().AddAsync(supplier);
        }

        public async Task UpdateAsync(Supplier supplier)
        {
            _context.Set<Supplier>().Update(supplier);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<List<SupplierDto>> SuggestSuppliersAsync(string term)
        {
            return await _context._Suppliers
                .Where(s => s.AccountNo.Contains(term) || s.Name.Contains(term))
                .OrderBy(s => s.AccountNo)
                .Take(10)
                .Select(s => new SupplierDto
                {
                    AccountNo = s.AccountNo,
                    Name = s.Name
                })
                .ToListAsync();
        }

        public async Task<string?> GetLastAccountNumberAsync()
        {
            var values = await _context._Suppliers
                .Where(x => !string.IsNullOrWhiteSpace(x.AccountNo))
                .Select(x => x.AccountNo)
                .ToListAsync();

            return SequenceHelper.GetHighestNumericCode(values);
        }

    }
}

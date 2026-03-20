using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly BackOfficeAdminContext _context;
        public CustomerRepository(BackOfficeAdminContext context)
        {
            _context = context;
        }

        public async Task<Customer?> GetByAccNoAsync(string accNo)
        {
            return await _context.Customers.FirstOrDefaultAsync(x => x.AccNo == accNo);
        }
        public async Task<List<Customer>> GetAllAsync()
        {
            return await _context.Customers
                .OrderBy(x => x.AccNo)
                .ToListAsync();
        }
        public async Task AddAsync(Customer entity)
        {
            await _context.Customers.AddAsync(entity);
        }

        public async Task UpdateAsync(Customer entity)
        {
            _context.Customers.Update(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<string?> GetLastAccountNumberAsync()
        {
            var values = await _context.Customers
                .Where(x => !string.IsNullOrWhiteSpace(x.AccNo))
                .Select(x => x.AccNo)
                .ToListAsync();

            return SequenceHelper.GetHighestNumericCode(values);
        }
    }
}

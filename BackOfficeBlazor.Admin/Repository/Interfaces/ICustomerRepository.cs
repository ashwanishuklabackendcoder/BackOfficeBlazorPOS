using BackOfficeBlazor.Admin.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByAccNoAsync(string accNo);
        Task<List<Customer>> GetAllAsync();
        Task AddAsync(Customer entity);
        Task UpdateAsync(Customer entity);
        Task SaveChangesAsync();
        Task<string?> GetLastAccountNumberAsync();
    }
}

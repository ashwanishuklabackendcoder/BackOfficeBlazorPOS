using BackOfficeBlazor.Admin.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<string?> GetLastCodeAsync();
        // Add future custom queries here
    }
}

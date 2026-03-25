using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private readonly BackOfficeAdminContext _context;

        public CategoryRepository(BackOfficeAdminContext context) : base(context)
        {
            _context = context;
        }

        public async Task<string?> GetLastCodeAsync()
        {
            var values = await _context._Categories
                .Where(x => !string.IsNullOrWhiteSpace(x.Code))
                .Select(x => x.Code)
                .ToListAsync();

            return SequenceHelper.GetHighestNumericCode(values);
        }
    }
}

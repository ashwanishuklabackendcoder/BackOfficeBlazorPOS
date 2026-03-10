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
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly BackOfficeAdminContext _context;

        public GenericRepository(BackOfficeAdminContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<T>> GetAllAsync() =>
            await _context.Set<T>().ToListAsync();

        public async Task<T?> GetByIdAsync(int id) =>
            await _context.Set<T>().FindAsync(id);
        public Task<Category?> GetByCodeAsync(string code)
        => _context._Categories.FirstOrDefaultAsync(x => x.Code == code);

        public async Task AddAsync(T entity)
        {
            _context.Set<T>().Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string code)
        {
            var entity = await GetByCodeAsync(code);
            if (entity != null)
            {
                _context.Set<Category>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<CategoryDto>> GetAllCategory(string level)
        {
            var query = _context._Categories
                .Where(x => x.IsDeleted == false);

            if (level == "A")
                query = query.Where(x => x.A == true);
            else if (level == "B")
                query = query.Where(x => x.B == true);
            else if (level == "C")
                query = query.Where(x => x.C == true);

            return await query
                .OrderBy(x => x.Code)
                .Select(x => new CategoryDto
                {
                    Code = x.Code,
                    Name = x.Name,
                    A = x.A,
                    B = x.B,
                    C = x.C,
                    Major = x.Major
                })
                .ToListAsync();
        }

        public async Task<List<Category>> SuggestAsync(string type, string query)
        {
            IQueryable<Category> q = _context._Categories;

            // Apply A/B/C boolean filtering
            q = type switch
            {
                "A" => q.Where(x => x.A == true),
                "B" => q.Where(x => x.B == true),
                "C" => q.Where(x => x.C == true),
                _ => q.Where(x => false)   // invalid type returns empty list
            };

            // Apply text search
            q = q.Where(x =>
                x.Code.Contains(query) ||
                x.Name.Contains(query));

            return await q
                .OrderBy(x => x.Name)
                .Take(20)
                .ToListAsync();
        }
    }
}

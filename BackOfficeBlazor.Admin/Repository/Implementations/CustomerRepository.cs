using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<List<Customer>> SearchAsync(CustomerSearchRequestDto request)
        {
            var query = _context.Customers.AsNoTracking().AsQueryable();

            if (request.CurrentOnly)
                query = query.Where(x => !x.Stop);

            var matchType = NormalizeMatchType(request.MatchType);

            query = ApplyField(query, request.Surname, matchType, x => x.Surname);
            query = ApplyField(query, request.Name, matchType, x => x.Firstname);
            query = ApplyField(query, request.Title, matchType, x => x.Title);
            query = ApplyField(query, request.Initials, matchType, x => x.Initials);
            query = ApplyField(query, request.AccNo, matchType, x => x.AccNo);
            query = ApplyField(query, request.PostCode, matchType, x => x.Postcode);
            query = ApplyField(query, request.LoyaltyNo, matchType, x => x.LoyaltyNo);
            query = ApplyField(query, request.Mobile, matchType, x => x.Mobile);
            query = ApplyField(query, request.Telephone, matchType, x => x.Telephone);
            query = ApplyField(query, request.Email, matchType, x => x.Email);

            if (!string.IsNullOrWhiteSpace(request.Address))
            {
                var value = request.Address.Trim();
                query = matchType switch
                {
                    "equal" => query.Where(x =>
                        x.HouseName == value ||
                        x.Address1 == value ||
                        x.Address2 == value ||
                        x.Address3 == value ||
                        x.Address4 == value ||
                        x.DeliveryHousename == value ||
                        x.DeliveryAddress1 == value ||
                        x.DeliveryAddress2 == value ||
                        x.DeliveryAddress3 == value ||
                        x.DeliveryAddress4 == value),
                    "startswith" => query.Where(x =>
                        (x.HouseName ?? string.Empty).StartsWith(value) ||
                        (x.Address1 ?? string.Empty).StartsWith(value) ||
                        (x.Address2 ?? string.Empty).StartsWith(value) ||
                        (x.Address3 ?? string.Empty).StartsWith(value) ||
                        (x.Address4 ?? string.Empty).StartsWith(value) ||
                        (x.DeliveryHousename ?? string.Empty).StartsWith(value) ||
                        (x.DeliveryAddress1 ?? string.Empty).StartsWith(value) ||
                        (x.DeliveryAddress2 ?? string.Empty).StartsWith(value) ||
                        (x.DeliveryAddress3 ?? string.Empty).StartsWith(value) ||
                        (x.DeliveryAddress4 ?? string.Empty).StartsWith(value)),
                    "endswith" => query.Where(x =>
                        (x.HouseName ?? string.Empty).EndsWith(value) ||
                        (x.Address1 ?? string.Empty).EndsWith(value) ||
                        (x.Address2 ?? string.Empty).EndsWith(value) ||
                        (x.Address3 ?? string.Empty).EndsWith(value) ||
                        (x.Address4 ?? string.Empty).EndsWith(value) ||
                        (x.DeliveryHousename ?? string.Empty).EndsWith(value) ||
                        (x.DeliveryAddress1 ?? string.Empty).EndsWith(value) ||
                        (x.DeliveryAddress2 ?? string.Empty).EndsWith(value) ||
                        (x.DeliveryAddress3 ?? string.Empty).EndsWith(value) ||
                        (x.DeliveryAddress4 ?? string.Empty).EndsWith(value)),
                    _ => query.Where(x =>
                        (x.HouseName ?? string.Empty).Contains(value) ||
                        (x.Address1 ?? string.Empty).Contains(value) ||
                        (x.Address2 ?? string.Empty).Contains(value) ||
                        (x.Address3 ?? string.Empty).Contains(value) ||
                        (x.Address4 ?? string.Empty).Contains(value) ||
                        (x.DeliveryHousename ?? string.Empty).Contains(value) ||
                        (x.DeliveryAddress1 ?? string.Empty).Contains(value) ||
                        (x.DeliveryAddress2 ?? string.Empty).Contains(value) ||
                        (x.DeliveryAddress3 ?? string.Empty).Contains(value) ||
                        (x.DeliveryAddress4 ?? string.Empty).Contains(value))
                };
            }

            return await query
                .OrderBy(x => x.Surname)
                .ThenBy(x => x.Firstname)
                .ThenBy(x => x.AccNo)
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

        private static IQueryable<Customer> ApplyField(
            IQueryable<Customer> query,
            string? rawValue,
            string matchType,
            System.Linq.Expressions.Expression<System.Func<Customer, string?>> selector)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return query;

            var value = rawValue.Trim();

            return matchType switch
            {
                "equal" => query.Where(BuildEquals(selector, value)),
                "startswith" => query.Where(BuildStartsWith(selector, value)),
                "endswith" => query.Where(BuildEndsWith(selector, value)),
                _ => query.Where(BuildContains(selector, value))
            };
        }

        private static System.Linq.Expressions.Expression<System.Func<Customer, bool>> BuildContains(
            System.Linq.Expressions.Expression<System.Func<Customer, string?>> selector,
            string value)
        {
            var parameter = selector.Parameters[0];
            var body = System.Linq.Expressions.Expression.Call(
                System.Linq.Expressions.Expression.Coalesce(selector.Body, System.Linq.Expressions.Expression.Constant(string.Empty)),
                typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!,
                System.Linq.Expressions.Expression.Constant(value));
            return System.Linq.Expressions.Expression.Lambda<System.Func<Customer, bool>>(body, parameter);
        }

        private static System.Linq.Expressions.Expression<System.Func<Customer, bool>> BuildStartsWith(
            System.Linq.Expressions.Expression<System.Func<Customer, string?>> selector,
            string value)
        {
            var parameter = selector.Parameters[0];
            var body = System.Linq.Expressions.Expression.Call(
                System.Linq.Expressions.Expression.Coalesce(selector.Body, System.Linq.Expressions.Expression.Constant(string.Empty)),
                typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) })!,
                System.Linq.Expressions.Expression.Constant(value));
            return System.Linq.Expressions.Expression.Lambda<System.Func<Customer, bool>>(body, parameter);
        }

        private static System.Linq.Expressions.Expression<System.Func<Customer, bool>> BuildEndsWith(
            System.Linq.Expressions.Expression<System.Func<Customer, string?>> selector,
            string value)
        {
            var parameter = selector.Parameters[0];
            var body = System.Linq.Expressions.Expression.Call(
                System.Linq.Expressions.Expression.Coalesce(selector.Body, System.Linq.Expressions.Expression.Constant(string.Empty)),
                typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) })!,
                System.Linq.Expressions.Expression.Constant(value));
            return System.Linq.Expressions.Expression.Lambda<System.Func<Customer, bool>>(body, parameter);
        }

        private static System.Linq.Expressions.Expression<System.Func<Customer, bool>> BuildEquals(
            System.Linq.Expressions.Expression<System.Func<Customer, string?>> selector,
            string value)
        {
            var parameter = selector.Parameters[0];
            var body = System.Linq.Expressions.Expression.Equal(
                selector.Body,
                System.Linq.Expressions.Expression.Constant(value, typeof(string)));
            return System.Linq.Expressions.Expression.Lambda<System.Func<Customer, bool>>(body, parameter);
        }

        private static string NormalizeMatchType(string? value)
        {
            var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
            return normalized.Replace(" ", string.Empty) switch
            {
                "startswith" => "startswith",
                "endswith" => "endswith",
                "equal" => "equal",
                _ => "contains"
            };
        }
    }
}

using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Shared.DTOs;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    internal static class StaffUserMapper
    {
        public static StaffUserDto ToDto(StaffUser user)
        {
            return new StaffUserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                StaffCode = user.StaffCode,
                IsAdmin = user.IsAdmin,
                IsActive = user.IsActive,
                Permissions = ToPermissionDto(user.Permission),
                PermissionKeys = ResolvePermissionKeys(user)
            };
        }

        private static List<string> ResolvePermissionKeys(StaffUser user)
        {
            var keys = user.PermissionEntries?
                .Select(x => x.PermissionKey)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (keys != null && keys.Count > 0)
                return keys;

            return PermissionCatalog.ToKeys(ToPermissionDto(user.Permission));
        }

        public static StaffUserPermissionDto ToPermissionDto(StaffUserPermission? p)
        {
            if (p == null)
                return new StaffUserPermissionDto();

            return new StaffUserPermissionDto
            {
                Till = p.Till,
                Workshop = p.Workshop,
                ProductAdd = p.ProductAdd,
                StockInput = p.StockInput,
                Layaway = p.Layaway,
                CustomerSalesReturn = p.CustomerSalesReturn,
                Category = p.Category,
                Brand = p.Brand,
                Supplier = p.Supplier,
                Customer = p.Customer
            };
        }

        public static void ApplyPermissions(StaffUserPermission entity, StaffUserPermissionDto dto)
        {
            entity.Till = dto.Till;
            entity.Workshop = dto.Workshop;
            entity.ProductAdd = dto.ProductAdd;
            entity.StockInput = dto.StockInput;
            entity.Layaway = dto.Layaway;
            entity.CustomerSalesReturn = dto.CustomerSalesReturn;
            entity.Category = dto.Category;
            entity.Brand = dto.Brand;
            entity.Supplier = dto.Supplier;
            entity.Customer = dto.Customer;
        }

        public static List<string> NormalizeKeys(IEnumerable<string>? keys)
        {
            if (keys == null)
                return new List<string>();

            return keys
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Where(x => PermissionCatalog.AllKeys.Contains(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}

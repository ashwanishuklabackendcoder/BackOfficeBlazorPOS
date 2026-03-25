using System.Security.Claims;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Shared.DTOs;

namespace POSAPI.Security
{
    public class ApiAccessService
    {
        private readonly IStaffUserRepository _staffUsers;

        public ApiAccessService(IStaffUserRepository staffUsers)
        {
            _staffUsers = staffUsers;
        }

        public bool IsAdmin(ClaimsPrincipal user)
            => bool.TryParse(user.FindFirst("is_admin")?.Value, out var isAdmin) && isAdmin;

        public async Task<bool> HasAnyPermissionAsync(ClaimsPrincipal user, params string[] permissionKeys)
        {
            if (IsAdmin(user))
                return true;

            if (!int.TryParse(user.FindFirst("user_id")?.Value, out var userId))
                return false;

            var staffUser = await _staffUsers.GetByIdAsync(userId);
            if (staffUser == null || !staffUser.IsActive)
                return false;

            var assignedKeys = staffUser.PermissionEntries
                .Select(x => x.PermissionKey)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (assignedKeys.Count == 0 && staffUser.Permission != null)
            {
                var legacyPermissions = new StaffUserPermissionDto
                {
                    Till = staffUser.Permission.Till,
                    Workshop = staffUser.Permission.Workshop,
                    ProductAdd = staffUser.Permission.ProductAdd,
                    StockInput = staffUser.Permission.StockInput,
                    Layaway = staffUser.Permission.Layaway,
                    CustomerSalesReturn = staffUser.Permission.CustomerSalesReturn,
                    Category = staffUser.Permission.Category,
                    Brand = staffUser.Permission.Brand,
                    Supplier = staffUser.Permission.Supplier,
                    Customer = staffUser.Permission.Customer
                };

                foreach (var key in PermissionCatalog.ToKeys(legacyPermissions))
                    assignedKeys.Add(key);
            }

            return permissionKeys.Any(assignedKeys.Contains);
        }
    }
}

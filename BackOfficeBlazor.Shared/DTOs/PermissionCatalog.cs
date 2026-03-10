namespace BackOfficeBlazor.Shared.DTOs
{
    public static class PermissionKeys
    {
        public const string Till = "till";
        public const string Workshop = "workshop";
        public const string ProductAdd = "product.add";
        public const string StockInput = "stock.input";
        public const string Layaway = "layaway";
        public const string CustomerSalesReturn = "customer.sales_return";
        public const string Category = "maintenance.category";
        public const string Brand = "maintenance.brand";
        public const string Supplier = "maintenance.supplier";
        public const string Customer = "maintenance.customer";
        public const string ProductList = "maintenance.productlist";
    }

    public sealed class PermissionDefinition
    {
        public string Key { get; set; } = "";
        public string Label { get; set; } = "";
        public string Group { get; set; } = "";
    }

    public static class PermissionCatalog
    {
        public static readonly List<PermissionDefinition> All = new()
        {
            new() { Key = PermissionKeys.Category, Label = "Categories", Group = "Maintenance" },
            new() { Key = PermissionKeys.Brand, Label = "Brand", Group = "Maintenance" },
            new() { Key = PermissionKeys.Supplier, Label = "Supplier", Group = "Maintenance" },
            new() { Key = PermissionKeys.ProductList, Label = "ProductList", Group = "Maintenance" },
            new() { Key = PermissionKeys.Customer, Label = "Customers", Group = "Maintenance" },
            new() { Key = PermissionKeys.ProductAdd, Label = "Add New Product", Group = "Products" },
            new() { Key = PermissionKeys.StockInput, Label = "Stock Input", Group = "Products" },
            new() { Key = PermissionKeys.Till, Label = "Till", Group = "Products" },
            new() { Key = PermissionKeys.Layaway, Label = "Layaway", Group = "Products" },
            new() { Key = PermissionKeys.CustomerSalesReturn, Label = "Customer Sale and Return", Group = "Products" },
            new() { Key = PermissionKeys.Workshop, Label = "Workshop", Group = "Products" }
        };

        public static readonly HashSet<string> AllKeys =
            All.Select(x => x.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);

        public static List<string> ToKeys(StaffUserPermissionDto dto)
        {
            var keys = new List<string>();
            if (dto.Till) keys.Add(PermissionKeys.Till);
            if (dto.Workshop) keys.Add(PermissionKeys.Workshop);
            if (dto.ProductAdd) keys.Add(PermissionKeys.ProductAdd);
            if (dto.StockInput) keys.Add(PermissionKeys.StockInput);
            if (dto.Layaway) keys.Add(PermissionKeys.Layaway);
            if (dto.CustomerSalesReturn) keys.Add(PermissionKeys.CustomerSalesReturn);
            if (dto.Category) keys.Add(PermissionKeys.Category);
            if (dto.Brand) keys.Add(PermissionKeys.Brand);
            if (dto.Supplier) keys.Add(PermissionKeys.Supplier);
           
            if (dto.Customer) keys.Add(PermissionKeys.Customer);
            return keys;
        }

        public static StaffUserPermissionDto ToLegacyDto(IEnumerable<string> keys)
        {
            var set = keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
            return new StaffUserPermissionDto
            {
                Till = set.Contains(PermissionKeys.Till),
                Workshop = set.Contains(PermissionKeys.Workshop),
                ProductAdd = set.Contains(PermissionKeys.ProductAdd),
                StockInput = set.Contains(PermissionKeys.StockInput),
                Layaway = set.Contains(PermissionKeys.Layaway),
                CustomerSalesReturn = set.Contains(PermissionKeys.CustomerSalesReturn),
                Category = set.Contains(PermissionKeys.Category),
                Brand = set.Contains(PermissionKeys.Brand),
                Supplier = set.Contains(PermissionKeys.Supplier),
                Customer = set.Contains(PermissionKeys.Customer)
            };
        }
    }
}

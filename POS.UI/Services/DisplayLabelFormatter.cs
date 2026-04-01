using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Services
{
    public static class DisplayLabelFormatter
    {
        public static string FormatCustomer(CustomerDto? customer, string? fallbackAccountNo = null)
        {
            var accountNo = NormalizeCustomerAccountNo(customer?.AccNo ?? fallbackAccountNo);
            var name = GetCustomerName(customer);

            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(accountNo))
                return $"{name} ({accountNo})";

            if (!string.IsNullOrWhiteSpace(name))
                return name;

            return accountNo;
        }

        public static string FormatCustomer(string? accountNo, IReadOnlyDictionary<string, CustomerDto>? customers)
        {
            var normalizedAccountNo = NormalizeCustomerAccountNo(accountNo);
            if (string.IsNullOrWhiteSpace(normalizedAccountNo))
                return string.Empty;

            if (customers != null && customers.TryGetValue(normalizedAccountNo, out var customer))
                return FormatCustomer(customer, normalizedAccountNo);

            return normalizedAccountNo;
        }

        public static string FormatLocation(LocationDto? location, string? fallbackCode = null)
        {
            var code = NormalizeLocationCode(location?.Code ?? fallbackCode);
            var name = location?.Name?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(name))
                return $"{code} - {name}";

            if (!string.IsNullOrWhiteSpace(name))
                return name;

            return code;
        }

        public static string FormatLocation(string? code, IReadOnlyDictionary<string, LocationDto>? locations)
        {
            var normalizedCode = NormalizeLocationCode(code);
            if (string.IsNullOrWhiteSpace(normalizedCode))
                return string.Empty;

            if (locations != null && locations.TryGetValue(normalizedCode, out var location))
                return FormatLocation(location, normalizedCode);

            return normalizedCode;
        }

        public static string FormatLocationList(
            IEnumerable<string>? codes,
            IReadOnlyDictionary<string, LocationDto>? locations,
            string emptyValue = "All")
        {
            if (codes == null)
                return emptyValue;

            var values = codes
                .Select(code => FormatLocation(code, locations))
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return values.Count == 0 ? emptyValue : string.Join(", ", values);
        }

        public static string GetCustomerName(CustomerDto? customer)
        {
            if (customer == null)
                return string.Empty;

            var parts = new[] { customer.Title, customer.Initials, customer.Firstname, customer.Surname }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!.Trim());

            return string.Join(" ", parts);
        }

        public static string NormalizeCustomerAccountNo(string? accountNo)
            => string.IsNullOrWhiteSpace(accountNo) ? string.Empty : accountNo.Trim();

        public static string NormalizeLocationCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return string.Empty;

            var trimmed = code.Trim().ToUpperInvariant();
            return int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                ? value.ToString("D2", CultureInfo.InvariantCulture)
                : trimmed;
        }
    }
}

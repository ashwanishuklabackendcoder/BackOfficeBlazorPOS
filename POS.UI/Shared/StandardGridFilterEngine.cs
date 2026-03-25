using System;
using System.Collections.Generic;
using System.Linq;
using BackOfficeBlazor.Shared.DTOs;
using POS.UI.Models;

namespace POS.UI.Shared
{
    public static class StandardGridFilterEngine
    {
        public static IEnumerable<TItem> ApplyFilters<TItem>(
            IEnumerable<TItem> data,
            IEnumerable<GridColumn<TItem>> columns,
            IDictionary<string, ColumnFilter> filters)
        {
            foreach (var column in columns)
            {
                if (!filters.TryGetValue(column.ColumnKey, out var filter))
                    continue;

                if (!HasActiveFilter(filter))
                    continue;

                data = data.Where(item => ColumnMatchesFilter(column, filter, item));
            }

            return data;
        }

        private static bool HasActiveFilter(ColumnFilter filter)
        {
            return !string.IsNullOrWhiteSpace(filter.Text) ||
                   !string.IsNullOrWhiteSpace(filter.SelectedOption) ||
                   filter.Min.HasValue ||
                   filter.Max.HasValue ||
                   filter.From.HasValue ||
                   filter.To.HasValue;
        }

        private static bool ColumnMatchesFilter<TItem>(GridColumn<TItem> column, ColumnFilter filter, TItem item)
        {
            var raw = column.Value(item);

            switch (column.ColumnFilterType)
            {
                case GridFilterType.NumericRange:
                    if (!TryConvertDecimal(raw, out var numeric))
                        return false;

                    if (filter.Min.HasValue && numeric < filter.Min.Value)
                        return false;
                    if (filter.Max.HasValue && numeric > filter.Max.Value)
                        return false;

                    return true;
                case GridFilterType.DateRange:
                    if (!TryConvertDate(raw, out var date))
                        return false;

                    if (filter.From.HasValue && date < filter.From.Value.Date)
                        return false;
                    if (filter.To.HasValue && date > filter.To.Value.Date)
                        return false;

                    return true;
                case GridFilterType.Dropdown:
                    if (string.IsNullOrWhiteSpace(filter.SelectedOption))
                        return true;

                    return string.Equals(filter.SelectedOption.Trim(), raw?.ToString()?.Trim(), StringComparison.OrdinalIgnoreCase);
                default:
                    if (string.IsNullOrWhiteSpace(filter.Text))
                        return true;

                    var text = raw?.ToString() ?? "";
                    return text.Contains(filter.Text.Trim(), StringComparison.OrdinalIgnoreCase);
            }
        }

        private static bool TryConvertDecimal(object? value, out decimal result)
        {
            if (value is decimal d)
            {
                result = d;
                return true;
            }

            if (value is IConvertible conv)
            {
                try
                {
                    result = conv.ToDecimal(System.Globalization.CultureInfo.InvariantCulture);
                    return true;
                }
                catch
                {
                }
            }

            result = 0;
            return false;
        }

        private static bool TryConvertDate(object? value, out DateTime result)
        {
            if (value is DateTime dt)
            {
                result = dt;
                return true;
            }

            if (value is string text && DateTime.TryParse(text, out var parsed))
            {
                result = parsed;
                return true;
            }

            result = DateTime.MinValue;
            return false;
        }
    }
}


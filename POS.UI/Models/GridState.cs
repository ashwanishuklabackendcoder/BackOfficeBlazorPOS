using System;
using System.Collections.Generic;
using BackOfficeBlazor.Shared.DTOs;

namespace POS.UI.Models
{
    public class GridState
    {
        public Dictionary<string, ColumnFilter> Filters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> HiddenColumns { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public string? SortColumnKey { get; set; }
        public bool SortDescending { get; set; }
        public bool SortAscending
        {
            get => !SortDescending;
            set => SortDescending = !value;
        }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 0;
    }

    public class ColumnFilter
    {
        public string? Text { get; set; }
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? SelectedOption { get; set; }
    }

    public class ColumnFilterChange
    {
        public string ColumnKey { get; }
        public ColumnFilter Filter { get; }

        public ColumnFilterChange(string columnKey, ColumnFilter filter)
        {
            ColumnKey = columnKey;
            Filter = filter;
        }
    }

}

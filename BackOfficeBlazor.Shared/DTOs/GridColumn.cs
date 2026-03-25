using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class GridColumn<TItem>
    {
        public string Title { get; set; } = "";
        public string? Key { get; set; }
        public Func<TItem, object?> Value { get; set; } = default!;
        public bool Sortable { get; set; } = true;
        public bool Filterable { get; set; } = true;
        public bool IsHideable { get; set; } = true;
        public bool IsVisible { get; set; } = true;
        public IEnumerable<string>? FilterOptions { get; set; }
        public IEnumerable<string>? LookupValues { get; set; }
        public string? FilterType { get; set; }

        public string ColumnKey => string.IsNullOrWhiteSpace(Key) ? Title : Key!;
        public string Field
        {
            get => ColumnKey;
            set => Key = value;
        }

        private GridFilterType _filterType = GridFilterType.Text;
        public GridFilterType ColumnFilterType
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(FilterType))
                {
                    return Enum.TryParse<GridFilterType>(FilterType, true, out var parsed)
                        ? parsed
                        : GridFilterType.Text;
                }

                return _filterType;
            }
            set => _filterType = value;
        }
    }
}

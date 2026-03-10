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
        public Func<TItem, object> Value { get; set; } = default!;
        public bool Sortable { get; set; } = true;
    }
}

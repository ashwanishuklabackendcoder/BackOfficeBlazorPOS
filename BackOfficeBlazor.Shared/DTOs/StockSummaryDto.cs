using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class StockSummaryDto
    {
        public string PartNumber { get; set; } = null!;
        public int TotalStock { get; set; }
    }
}

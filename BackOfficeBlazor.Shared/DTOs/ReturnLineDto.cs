using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class ReturnLineDto
    {
        public string PartNumber { get; set; } = "";

        public string? StockNo { get; set; }   // For Major Items

        public int Qty { get; set; }
        public string Location { get; set; } = "";   // ⭐ ADD THIS

        public string Terminal { get; set; } = "";
        public decimal Sell { get; set; }

        public string Reason { get; set; } = "";
    }
}


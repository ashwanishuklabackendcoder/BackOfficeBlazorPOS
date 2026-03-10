using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class StockHistoryDto
    {
        public string PartNumber { get; set; } = null!;
        public string LocationCode { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Cost { get; set; }
        public DateTime DateCreated { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Entities
{
    public class ProductStockMovement
    {
        public int Id { get; set; }
        public DateTime DateAndTime { get; set; }
        public string PartNo { get; set; } = "";
        public int StockQty { get; set; }
        public decimal? Cost { get; set; }
        public string? StockNumber { get; set; }
        public string? SerialNumber { get; set; }
        public string Notes { get; set; } = "";
        public string SalesCode { get; set; } = "";
        public string FromLocation { get; set; } = "";
        public string ToLocation { get; set; } = "";
        public int TotalCurrentStock { get; set; }
    }

}

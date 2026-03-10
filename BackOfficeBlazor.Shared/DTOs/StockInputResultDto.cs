using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class StockInputResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";

        public string PartNumber { get; set; } = null!;
        public string LocationCode { get; set; } = null!;

        public int QuantityAdded { get; set; }

        /// <summary>
        /// Only populated for Major products
        /// </summary>
        public List<string> GeneratedStockNumbers { get; set; } = new();

        public int NewStockLevel { get; set; }
    }

}

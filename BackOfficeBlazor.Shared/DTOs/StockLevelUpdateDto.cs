using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class StockLevelUpdateDto
    {
        public string PartNumber { get; set; } = null!;
        public string LocationCode { get; set; } = null!;

        /// <summary>
        /// Always positive for Stock In
        /// (negative when you implement Stock Out)
        /// </summary>
        public int DeltaQuantity { get; set; }

        public DateTime DateUpdated { get; set; }
    }

}

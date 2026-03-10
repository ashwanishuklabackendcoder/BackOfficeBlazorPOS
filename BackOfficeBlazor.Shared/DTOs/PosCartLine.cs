using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    class PosCartLine
    {
        public string PartNumber { get; set; }
        public string Description { get; set; }

        public int Qty { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal OriginalPrice { get; set; }

        public decimal DiscountPercent { get; set; }

        public decimal LineTotal =>
            Qty * UnitPrice * (1 - DiscountPercent / 100m);
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class ProductStockLevelDto
    {
        public string LocationCode { get; set; } = "";
        public int Quantity { get; set; }
    }

}

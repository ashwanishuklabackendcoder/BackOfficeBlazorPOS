using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class PosSaleRequestDto
    {
        public string Location { get; set; } = "";
        public string Terminal { get; set; } = "";
        public string SalesPerson { get; set; } = "";
        public string Customer { get; set; } = "";
        public decimal SubTotal { get; set; }
        public decimal VatAmount { get; set; }
        public decimal NetTotal { get; set; }

        public List<PosSaleLineDto> Lines { get; set; } = new();

        public PosPaymentDto Payment { get; set; } = new();
    }

}

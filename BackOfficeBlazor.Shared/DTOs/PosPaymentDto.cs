using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class PosPaymentDto
    {
        public decimal Cash { get; set; }
        public decimal Cheque { get; set; }
        public decimal MasterCard { get; set; }
        public decimal Visa { get; set; }
        public decimal Credit { get; set; }

        public decimal TotalTendered =>
            Cash + Cheque + MasterCard + Visa + Credit;
    }


}

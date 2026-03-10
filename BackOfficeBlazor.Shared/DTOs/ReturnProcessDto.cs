using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class ReturnProcessDto
    {
        public string InvoiceNo { get; set; } = "";

        public List<ReturnLineDto> Lines { get; set; } = new();

        public decimal RefundCash { get; set; }
        public decimal RefundCard { get; set; }

        public string Location { get; set; } = "";
        public string Terminal { get; set; } = "";
        public string Staff { get; set; } = "";
    }
}

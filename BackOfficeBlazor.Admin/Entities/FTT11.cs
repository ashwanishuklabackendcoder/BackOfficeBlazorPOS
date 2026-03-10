using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Entities
{
    public class FTT11
    {
        public int Id { get; set; }

        public string Location { get; set; } = "";
        public string InvoiceNumber { get; set; } = "";

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "";

        public decimal Cash { get; set; }
        public decimal Cheque { get; set; }

        public string Type3Description { get; set; } = "";
        public decimal Type3 { get; set; }

        public string Type4Description { get; set; } = "";
        public decimal Type4 { get; set; }

        public decimal Credit { get; set; }
    }

}

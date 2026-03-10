using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Entities
{
    public class FTT05
    {
        public int Id { get; set; }

        public DateTime? DateAndTime { get; set; }
        public string Date { get; set; } = "";
        public string Time { get; set; } = "";

        public string Location { get; set; } = "";
        public string InvoiceNumber { get; set; } = "";

        public int Count { get; set; }

        public string PartNumber { get; set; } = "";

        public decimal Cost { get; set; }
        public decimal Sell { get; set; }

        public string SalesPerson { get; set; } = "";
        public string InOut { get; set; } = "";
        public string DiscountCode { get; set; } = "";

        public decimal VAT { get; set; }
        public decimal Profit { get; set; }

        public string PaymentType { get; set; } = "";
        public int Quantity { get; set; }

        public string Customer { get; set; } = "";
        public string Terminal { get; set; } = "";

        public decimal Average { get; set; }
        public decimal Net { get; set; }

        public string Notes { get; set; } = "";
        public string Type { get; set; } = "";
        public string Band { get; set; } = "";
        public string Exempt { get; set; } = "";
        public string Source { get; set; } = "";

        public string StockNo { get; set; } = "";
        public string OrderNo { get; set; } = "";

        public string AI { get; set; } = "";
        public bool? BikmoYes { get; set; }
    }

}

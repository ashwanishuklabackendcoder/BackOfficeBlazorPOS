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
        public bool AddSellableItemsToStock { get; set; }

        public decimal RefundCash { get; set; }
        public decimal RefundMasterCard { get; set; }
        public decimal RefundVisa { get; set; }
        public decimal RefundCredit { get; set; }
        public decimal RefundCard => RefundMasterCard + RefundVisa;
        public decimal TotalRefund => RefundCash + RefundMasterCard + RefundVisa + RefundCredit;

        public string Location { get; set; } = "";
        public string Terminal { get; set; } = "";
        public string Staff { get; set; } = "";
    }
}

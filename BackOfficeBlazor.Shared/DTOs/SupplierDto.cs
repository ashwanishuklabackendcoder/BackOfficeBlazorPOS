using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class SupplierDto
    {
        [StringLength(6)]
        public string AccountNo { get; set; }

        [StringLength(30)]
        public string? Name { get; set; }

        [StringLength(30)]
        public string? Address1 { get; set; }

        [StringLength(30)]
        public string? Address2 { get; set; }

        [StringLength(30)]
        public string? Address3 { get; set; }

        [StringLength(30)]
        public string? Address4 { get; set; }

        [StringLength(8)]
        public string? Postcode { get; set; }

        [StringLength(15)]
        public string? Telephone { get; set; }

        [StringLength(15)]
        public string? Fax { get; set; }

        [StringLength(50)]
        public string? Email { get; set; }

        [StringLength(10)]
        public string? B2BFileName { get; set; }
        public int B2BFileType { get; set; }
        public bool B2BFileHasHeaderRow { get; set; }
        public bool B2BFileAppendLocationCode { get; set; }
        public decimal SettlementDiscount { get; set; } = 0.00m;
        public decimal CarriagePaidAmount { get; set; } = 0.00m;
    }
}

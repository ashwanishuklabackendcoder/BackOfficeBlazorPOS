using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Entities
{
    public class Supplier
    {
        [Key]
        [StringLength(6)]
        public string AccountNo { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string Address1 { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string Address2 { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string Address3 { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string Address4 { get; set; } = string.Empty;

        [Required, StringLength(8)]
        public string Postcode { get; set; } = string.Empty;

        [Required, StringLength(15)]
        public string Telephone { get; set; } = string.Empty;

        [Required, StringLength(15)]
        public string Fax { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(10)]
        public string B2BFileName { get; set; } = string.Empty;

        [Required]
        public int B2BFileType { get; set; }

        [Required]
        public bool B2BFileHasHeaderRow { get; set; }

        [Required]
        public bool B2BFileAppendLocationCode { get; set; }

        public decimal SettlementDiscount { get; set; }
        public decimal CarriagePaidAmount { get; set; }

        // Soft Delete Support
        public bool IsDeleted { get; set; } = false;

        // Audit fields
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime? DateUpdated { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Entities
{
    public class Location
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(10)]
        public string Code { get; set; } = string.Empty;

        [Required, StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string CompanyNumber { get; set; } = string.Empty;

        [StringLength(60)]
        public string? Address1 { get; set; }

        [StringLength(60)]
        public string? Address2 { get; set; }

        [StringLength(60)]
        public string? Address3 { get; set; }

        [StringLength(60)]
        public string? Address4 { get; set; }

        [Required, StringLength(12)]
        public string Postcode { get; set; } = string.Empty;

        [StringLength(80)]
        public string? GeneralEmailAddress { get; set; }

        [StringLength(30)]
        public string? MainTelephone { get; set; }

        [StringLength(80)]
        public string? AccountEmail { get; set; }

        [StringLength(80)]
        public string? AccountName { get; set; }

        [StringLength(80)]
        public string? AdminEmail { get; set; }

        [StringLength(80)]
        public string? AdminName { get; set; }

        [StringLength(120)]
        public string? StoreWebsiteURL { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public bool KeyLocation { get; set; } = false;

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? DateUpdated { get; set; }
    }
}

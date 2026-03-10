using System;
using System.ComponentModel.DataAnnotations;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class BranchDetailDto
    {
        public int Id { get; set; }

        [StringLength(2)]
        public string Code { get; set; } = string.Empty;

        [StringLength(30)]
        public string Name { get; set; } = string.Empty;

        [StringLength(30)]
        public string CompanyNumber { get; set; } = string.Empty;

        public string? Address1 { get; set; }

        public string? Address2 { get; set; }

        public string? Address3 { get; set; }

        public string? Address4 { get; set; }

        public string Postcode { get; set; } = string.Empty;

        public string? GeneralEmailAddress { get; set; }

        public string? MainTelephone { get; set; }

        [StringLength(200)]
        public string? AccountEmail { get; set; }

        [StringLength(200)]
        public string? AccountName { get; set; }

        [StringLength(200)]
        public string? AdminEmail { get; set; }

        [StringLength(200)]
        public string? AdminName { get; set; }

        [StringLength(200)]
        public string? StoreWebsiteURL { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool KeyLocation { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public BranchSettingsDto Settings { get; set; } = new();
    }
}

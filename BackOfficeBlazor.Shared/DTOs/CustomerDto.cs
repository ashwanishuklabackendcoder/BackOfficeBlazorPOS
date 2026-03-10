using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class CustomerDto
    {
        [Key]
        [StringLength(50)]
        public string? AccNo { get; set; }

        [StringLength(50)]
        public string? Title { get; set; }

        [StringLength(50)]
        public string? Initials { get; set; }

        [StringLength(50)]
        public string? Firstname { get; set; }

        [StringLength(50)]
        public string? Surname { get; set; }

        [StringLength(50)]
        public string? HouseName { get; set; }

        [StringLength(50)]
        public string? Postcode { get; set; }

        [StringLength(50)]
        public string? Telephone { get; set; }

        [StringLength(50)]
        public string? Mobile { get; set; }

        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(50)]
        public string? LoyaltyNo { get; set; }
        public decimal? Balance { get; set; }

        [StringLength(200)]
        public string? Address1 { get; set; }

        [StringLength(200)]
        public string? Address2 { get; set; }

        [StringLength(200)]
        public string? Address3 { get; set; }

        [StringLength(200)]
        public string? Address4 { get; set; }

        public bool Stop { get; set; }
        public bool SendLetter { get; set; }
        public decimal? CreditLimit { get; set; }
        public bool VATExempt { get; set; }
        [StringLength(5)]
        public string? Category { get; set; }
        public bool Autopay { get; set; }

        [StringLength(50)]
        public string? Country { get; set; }

        [StringLength(50)]
        public string? WorkPhone { get; set; }
        public DateTime? DOB { get; set; }

        public decimal? DiscountMinor { get; set; }
        public decimal? DiscountMajor { get; set; }

        [StringLength(5)]
        public string? DeliveryTitle { get; set; }

        [StringLength(5)]
        public string? DeliveryInitials { get; set; }

        [StringLength(50)]
        public string? DeliverySurname { get; set; }

        [StringLength(50)]
        public string? DeliveryFirstname { get; set; }

        [StringLength(50)]
        public string? DeliveryHousename { get; set; }

        [StringLength(200)]
        public string? DeliveryAddress1 { get; set; }

        [StringLength(200)]
        public string? DeliveryAddress2 { get; set; }

        [StringLength(200)]
        public string? DeliveryAddress3 { get; set; }

        [StringLength(200)]
        public string? DeliveryAddress4 { get; set; }

        [StringLength(15)]
        public string? DeliveryPostcode { get; set; }

        [StringLength(50)]
        public string? DeliveryCountry { get; set; }
    }
}


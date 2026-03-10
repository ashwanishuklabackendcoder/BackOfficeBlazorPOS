// BackOfficeBlazor.Admin/Entities/ProductItem.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace BackOfficeBlazor.Admin.Entities
{
    public class ProductItem
    {
        // PK is PartNumber, as per your table
        [Key]
        [StringLength(10)]
        public string PartNumber { get; set; } = string.Empty;

        [StringLength(50)]
        public string? MfrPartNumber { get; set; }

        [StringLength(50)]
        public string? MfrPartNumber2 { get; set; }

        public string? ImageMain { get; set; }
        public string? Image2 { get; set; }

        public string? GroupCode { get; set; }     // links all variants
        public bool IsVariant { get; set; }        // true = child, false = main
        public string? GroupName { get; set; }     // display name

        public string? Image3 { get; set; }
        public string? Image4 { get; set; }
        public bool Major { get; set; }     // Major / Minor
        public int? Gender { get; set; }     // static dropdown (Phase 1)
        public int? Suitability { get; set; }// static dropdown
        public DateTime? ExipryDate { get; set; }

        [StringLength(100)]
        public string? Make { get; set; }

        [StringLength(20)]
        public string? MakeCode { get; set; }

        [StringLength(350)]
        public string? Search1 { get; set; }

        [StringLength(350)]
        public string? Search2 { get; set; }

        public string? Details { get; set; }

        [StringLength(300)]
        public string? Size { get; set; }

        [StringLength(300)]
        public string? Color { get; set; }

        [StringLength(20)]
        public string? Barcode { get; set; }

        public bool? Current { get; set; }
        public int? PrintLabel { get; set; }
        public bool? AllowDiscount { get; set; }

        [StringLength(10)]
        public string? Year { get; set; }

        public int? BoxQuantity { get; set; }

        [StringLength(2)]
        public string? NominalSection { get; set; }

        [StringLength(4)]
        public string? NominalCode { get; set; }

        public int? Season { get; set; }     // static dropdown

        // Prices
        public decimal? CostPrice { get; set; }
        public int? DiscountPercentage { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Markup { get; set; }
        public int? VatCode { get; set; }    // fixed 1 = 20%
        public decimal? SuggestedRRP { get; set; }
        public decimal? StorePrice { get; set; }
        public decimal? TradePrice { get; set; }
        public decimal? MailOrderPrice { get; set; }
        public decimal? WebPrice { get; set; }

        [StringLength(10)]
        public string? OfferCode { get; set; }

        public bool? KeyItem { get; set; }
        public bool? AllowPoints { get; set; }
        public bool? Website { get; set; }
        public bool? WebOnly { get; set; }

        [StringLength(10)]
        public string? BinLocation1 { get; set; }

        [StringLength(10)]
        public string? BinLocation2 { get; set; }

        public int? PartsGarunteeMonths { get; set; }
        public int? LabourGarunteeMonths { get; set; }

        public decimal? Weight { get; set; }

        // Promo
        [StringLength(250)]
        public string? PromoName { get; set; }
        public decimal? PromoPrice { get; set; }
        public DateTime? PromoStart { get; set; }
        public DateTime? PromoEnd { get; set; }

        public int? MultibuyQuantity { get; set; }
        public decimal? MultibuySave { get; set; }

        // Suppliers & Categories (relations later)
        [StringLength(10)]
        public string? Supplier1Code { get; set; }

        [StringLength(10)]
        public string? Supplier2Code { get; set; }

        [StringLength(100)]
        public string? CatA { get; set; }
        [StringLength(50)]
        public string? CatACode { get; set; }

        [StringLength(100)]
        public string? CatB { get; set; }
        [StringLength(50)]
        public string? CatBCode { get; set; }

        [StringLength(100)]
        public string? CatC { get; set; }
        [StringLength(50)]
        public string? CatCCode { get; set; }

        // Web categories
        [StringLength(50)]
        public string? WebCat1Code { get; set; }
        [StringLength(50)]
        public string? WebCat2Code { get; set; }
        [StringLength(50)]
        public string? WebCat3Code { get; set; }
        [StringLength(50)]
        public string? WebCat4Code { get; set; }

        // Descriptions / E-commerce
        public string? ShortDescription { get; set; }
        public string? FullDescription { get; set; }
        [StringLength(50)]
        public string? WebsiteTitle { get; set; }
        [StringLength(50)]
        public string? EbayTitle { get; set; }
        [StringLength(50)]
        public string? GoogleShoppingTitle { get; set; }

        public string? Specifications { get; set; }
        public string? Geometry { get; set; }

        [StringLength(10)]
        public string? ItemId { get; set; }

        [StringLength(20)]
        public string? Range { get; set; }

        [StringLength(20)]
        public string? Finish { get; set; }

        public int? WebRef { get; set; }

        public bool? IsDiscontinued { get; set; }
        public bool? DoNotReOrder { get; set; }
        public bool? IsClearance { get; set; }
        public bool? IsFinalClearance { get; set; }

        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }

        // Notes fields etc. can be added later as needed.
    }
}

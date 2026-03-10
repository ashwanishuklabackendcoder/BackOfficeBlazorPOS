// Shared/DTOs/ProductDto.cs
using System.ComponentModel.DataAnnotations;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class ProductDto
    {
        [StringLength(10)]
        public string? PartNumber { get; set; }

        [StringLength(50)]
        public string? MfrPartNumber { get; set; }

        [StringLength(50)]
        public string? MfrPartNumber2 { get; set; }
        public bool Major { get; set; } = false;   // OFF by default
        public bool? Current { get; set; } = true; // ON by default
        public string? GroupCode { get; set; }     // links all variants
        public bool IsVariant { get; set; }        // true = child, false = main
        public string? GroupName { get; set; }     // display name

        [StringLength(10)]
        public string? OfferCode { get; set; }

        [StringLength(100)]
        public string? Make { get; set; }

        [StringLength(20)]
        public string? MakeCode { get; set; }

        [StringLength(350)]
        public string? Search1 { get; set; }

        [StringLength(350)]
        public string? Search2 { get; set; }
        public string? ImageMain { get; set; }
        public string? Image2 { get; set; }
        


        public string? Image3 { get; set; }
        public string? Image4 { get; set; }
        public string? Details { get; set; }
        [StringLength(300)]
        public string? Size { get; set; }

        [StringLength(300)]
        public string? Color { get; set; }

        [StringLength(20)]
        public string? Barcode { get; set; }
        public decimal? Weight { get; set; } = 0.00m;
        public bool HasGeometry { get; set; }
        public bool HasSpecification { get; set; }
        public decimal CostPrice { get; set; } = 0.00m;
        public decimal? Discount { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? Markup { get; set; }
        public int? VatCode { get; set; } = 1;
        public decimal? SuggestedRRP { get; set; } = 0.00m;
        public decimal? StorePrice { get; set; } = 0.00m;
        public decimal? TradePrice { get; set; } = 0.00m;
        public decimal? MailOrderPrice { get; set; } = 0.00m;
        public decimal? WebPrice { get; set; } = 0.00m;

       // public bool? Current { get; set; }
        public int? PrintLabel { get; set; }
        public bool? AllowDiscount { get; set; }

        //public int? Season { get; set; }
        [StringLength(10)]
        public string? Year { get; set; }
        public int? BoxQuantity { get; set; }
        public AgeRange Suitability { get; set; } = AgeRange.Any;
        public Genders Gender { get; set; } = Genders.Unisex;
        public Seasons Season { get; set; } = Seasons.All;
        [StringLength(250)]
        public string? PromoName { get; set; }

        [StringLength(20)]
        public string? Range { get; set; }

        [StringLength(20)]
        public string? Finish { get; set; }
        public decimal PromoPrice { get; set; } = 0.00m;
        public DateTime? PromoStart { get; set; }
        public DateTime? PromoEnd { get; set; }

        [StringLength(10)]
        public string? Supplier1Code { get; set; }

        [StringLength(10)]
        public string? Supplier2Code { get; set; }

        [StringLength(50)]
        public string? CatACode { get; set; }

        [StringLength(50)]
        public string? CatBCode { get; set; }

        [StringLength(50)]
        public string? CatCCode { get; set; }

        [StringLength(100)]
        public string? CatA { get; set; }

        [StringLength(100)]
        public string? CatB { get; set; }

        [StringLength(100)]
        public string? CatC { get; set; }
        public bool? KeyItem { get; set; }
        public bool? AllowPoints { get; set; }
        public bool? Website { get; set; }
        public bool? WebOnly { get; set; }

        public string? ShortDescription { get; set; }
        public string? FullDescription { get; set; }
        public string? Specifications { get; set; }
        public string? Geometry { get; set; }
        public int WebRef { get; set; }
        public enum Genders
        {
            Unisex=1,
            Male=2,
            Female=3
        }
        public enum AgeRange
        {
            Any=0,
            Adult=1,
            Child=2
        }
        public enum Seasons
        {
            All = 0,
            Winter = 1,
            Summer = 2
        }
        public bool IsUsingPromoPrice()
        {
            var now = DateTime.UtcNow;

            return PromoPrice > 0
                   && PromoStart.HasValue
                   && PromoStart.Value <= now
                   && (!PromoEnd.HasValue || PromoEnd.Value >= now);
        }
        public string? Notes { get; set; } // for Product Notes tab if you want
    }

    public class ProductLevelDto
    {
        public string PartNumber { get; set; } = string.Empty;

        public int Min01 { get; set; }
        public int Max01 { get; set; }
        public bool Replenish01 { get; set; }

        public int Min02 { get; set; }
        public int Max02 { get; set; }
        public bool Replenish02 { get; set; }

        public int Min03 { get; set; }
        public int Max03 { get; set; }
        public bool Replenish03 { get; set; }

        // extend as needed
    }
}

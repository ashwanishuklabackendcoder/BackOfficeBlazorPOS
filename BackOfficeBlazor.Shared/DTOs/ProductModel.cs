using System;

public class ProductModel
{
    public string Type { get; set; } = "Goods";
    public string Name { get; set; } = string.Empty;//Not in db
    public string Sku { get; set; } = string.Empty;//part no
    public string Unit { get; set; } = string.Empty;//not in db
    public bool Returnable { get; set; } = true;//not in db

    public string[] Units { get; set; } = new[] { "Box", "Pack", "Unit" }; //not in db
    public enum Genders
    {
        Unisex,
        Male,
        Female
    }

    public enum ClickCollect
    {
        InStoreAndHomeDelivery,
        InStoreOnly,
        HomeDeliveryOnly
    }

    public enum LeadTimes
    {
        Unknown = 0,
        NextDay = 1,
        SameDay = 2,
        Days3To5 = 3,
        Days7To10 = 4,
        Weeks2 = 5,
        Weeks3To4 = 6,
        OneMonth = 7,
        CallForAvailability = 8
    }

    public enum Seasons
    {
        All=0,
        Winter=1,
        Summer = 2
    }

    public enum AgeRange
    {
        Any,
        Adult,
        Child
    }

    public enum PrintLabels
    {
        Yes,
        No,
        One
    }

    public enum DeliveryOptions
    {
        Standard = 0,
        Free = 1,
        Premium = 2
    }

    public string? PartNumber { get; set; }
    public string? MfrPartNumber { get; set; }
    public bool Major { get; set; }
    public Genders Gender { get; set; } = Genders.Unisex;
    public AgeRange Suitability { get; set; } = AgeRange.Any;
    public DateTime? ExipryDate { get; set; }
    public string? MakeCode { get; set; }
    public string? Search1 { get; set; }
    public string? Search2 { get; set; }
    public string? Details { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public string? Barcode { get; set; }
    public bool Current { get; set; } = true;
    public PrintLabels PrintLabel { get; set; } = PrintLabels.Yes;
    public bool AllowDiscount { get; set; } = true;
    public string? Year { get; set; }
    public int BoxQuantity { get; set; }
    public string? NominalSection { get; set; }
    public string? NominalCode { get; set; }
    public Seasons Season { get; set; } = Seasons.All;

    public decimal CostPrice { get; set; }
    public decimal Discount { get; set; }
    public int DiscountPercentage { get; set; }
    public decimal Markup { get; set; }
    public int VatCode { get; set; }
    public decimal SuggestedRRP { get; set; }
    public decimal StorePrice { get; set; }
    public decimal TradePrice { get; set; }
    public decimal MailOrderPrice { get; set; }
    public decimal WebPrice { get; set; }

    public string? OfferCode { get; set; }
    public bool KeyItem { get; set; }
    public bool AllowPoints { get; set; }
    public bool Website { get; set; } = true;
    public decimal Weight { get; set; }


    public decimal PromoPrice { get; set; }
    public DateTime? PromoStart { get; set; }
    public DateTime? PromoEnd { get; set; }
    public DateTime? EstimatedArrivalDate { get; set; }

    public string? Supplier1Code { get; set; }
    public string? Supplier2Code { get; set; }

    public string? CatACode { get; set; }
    public string? CatBCode { get; set; }
    public string? CatCCode { get; set; }

    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }
    public string? Specifications { get; set; }
    public string? Geometry { get; set; }

    public int WebRef { get; set; }
    public string? Make { get; set; }
    public string? CatA { get; set; }
    public string? CatB { get; set; }
    public string? CatC { get; set; }
    public string? MfrPartNumber2 { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
    public string? PromoName { get; set; }
    public DeliveryOptions DeliveryOption { get; set; } = DeliveryOptions.Standard;
    public decimal BoxCost { get; set; }
    public bool IsVariation { get; set; }
}

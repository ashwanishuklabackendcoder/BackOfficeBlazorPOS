using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class ProductFilterDto
    {
        //public string? PartNumber { get; set; }
        //public string? Barcode { get; set; }
        [StringLength(10)]
        public string? Year { get; set; }

        [StringLength(100)]
        public string? Make { get; set; }

        [StringLength(350)]
        public string? Search1 { get; set; }

        [StringLength(350)]
        public string? Search2 { get; set; }

        [StringLength(50)]
        public string? MfrPartNumber { get; set; }

        [StringLength(10)]
        public string? Supplier1Code { get; set; }

        [StringLength(100)]
        public string? CatA { get; set; }

        [StringLength(100)]
        public string? CatB { get; set; }

        [StringLength(100)]
        public string? CatC { get; set; }

        [StringLength(50)]
        public string? CatACode { get; set; }

        [StringLength(50)]
        public string? CatBCode { get; set; }

        [StringLength(50)]
        public string? CatCCode { get; set; }
        public bool? Website { get; set; } = false;
        public decimal? CostPrice { get; set; } = 0.00m;
        public decimal? pricefrom { get; set; } = 0.00m;
        public decimal? priceto { get; set; } = 0.00m;
        public string? Details { get; set; }

        [StringLength(300)]
        public string? Size { get; set; }

        [StringLength(300)]
        public string? Color { get; set; }
        public int? Gender { get; set; }

        public string StockMode { get; set; } = "All";   // All, AllShops, Here, At, NotInStock
        [StringLength(10)]
        public string? StockLocation { get; set; }

        public string MatchType { get; set; } = "Contains";

    }

}

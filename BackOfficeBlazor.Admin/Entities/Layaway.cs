using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackOfficeBlazor.Admin.Entities
{
    [Table("Layaways")]
    public class Layaway
    {
        [Key]
        public int Id { get; set; }

        [Column("CustomeAccount")]
        [StringLength(50)]
        public string? CustomerAccNo { get; set; }

        public DateTime? Date { get; set; }

        [StringLength(5)]
        public string? Location { get; set; }

        [Column("SalesCode")]
        [StringLength(5)]
        public string? SalesPerson { get; set; }

        [Column("PartNo")]
        [StringLength(50)]
        public string? PartNumber { get; set; }

        public int? Quantity { get; set; }

        [Column("Price", TypeName = "numeric(18, 2)")]
        public decimal? Sell { get; set; }

        public bool? Reserved { get; set; }
        public int? LayawayType { get; set; }

        [StringLength(50)]
        public string? WorkshopJobNo { get; set; }

        public int? ReferenceNo { get; set; }

        [StringLength(10)]
        public string? StockNo { get; set; }

        [StringLength(300)]
        public string? Notes { get; set; }

        [Column("DiscountPercentage", TypeName = "numeric(18, 2)")]
        public decimal? DiscountPercentage { get; set; }
    }
}

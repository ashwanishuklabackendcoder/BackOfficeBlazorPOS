using System.ComponentModel.DataAnnotations;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class LayawayLineDto
    {
        [StringLength(50)]
        public string PartNumber { get; set; } = "";

        [StringLength(10)]
        public string StockNo { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Cost { get; set; }
        public decimal Sell { get; set; }
        public decimal Net { get; set; }
        public decimal Vat { get; set; }
        public bool IsMajor { get; set; }
    }
}

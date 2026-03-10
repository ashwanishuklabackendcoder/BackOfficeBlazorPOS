using System.ComponentModel.DataAnnotations;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class LayawayLineUpdateDto
    {
        [StringLength(50)]
        public string PartNumber { get; set; } = "";

        [StringLength(10)]
        public string StockNo { get; set; } = "";
        public int NewQty { get; set; }
    }
}

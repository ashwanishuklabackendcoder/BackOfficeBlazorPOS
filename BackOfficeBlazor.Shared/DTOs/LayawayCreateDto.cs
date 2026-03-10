using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class LayawayCreateDto
    {
        [StringLength(50)]
        public string CustomerAccNo { get; set; } = "";

        [StringLength(5)]
        public string Location { get; set; } = "";
        public string Terminal { get; set; } = "";

        [StringLength(5)]
        public string SalesPerson { get; set; } = "";
        public List<LayawayLineDto> Lines { get; set; } = new();
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class LayawayDetailDto
    {
        public int LayawayNo { get; set; }

        [StringLength(50)]
        public string CustomerAccNo { get; set; } = "";

        [StringLength(5)]
        public string Location { get; set; } = "";
        public string Terminal { get; set; } = "";

        [StringLength(5)]
        public string SalesPerson { get; set; } = "";
        public DateTime DateCreated { get; set; }
        public string Status { get; set; } = "";
        public List<LayawayLineDto> Lines { get; set; } = new();
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class StockTransferInputDto
    {
        [StringLength(10)]
        public string PartNumber { get; set; } = "";

        [StringLength(2)]
        public string FromLocation { get; set; } = "";

        [StringLength(2)]
        public string ToLocation { get; set; } = "";
        public int Quantity { get; set; }

        [StringLength(5)]
        public string SalesCode { get; set; } = "";

        [StringLength(300)]
        public string Notes { get; set; } = "";
    }

}

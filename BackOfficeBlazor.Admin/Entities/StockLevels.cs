using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Entities
{
    public class StockLevels
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(5)]
        public string PartNumber { get; set; } = null!;

        public int L01 { get; set; }
        public int L02 { get; set; }
        public int L03 { get; set; }
        public int L04 { get; set; }
        public int L05 { get; set; }
        public int L06 { get; set; }
        public int L07 { get; set; }
        public int L08 { get; set; }
        public int L09 { get; set; }
        public int L10 { get; set; }
        public int L11 { get; set; }
        public int L12 { get; set; }
        public int L13 { get; set; }
        public int L14 { get; set; }
        public int L15 { get; set; }
        public int L16 { get; set; }
        public int L17 { get; set; }
        public int L18 { get; set; }
        public int L19 { get; set; }
        public int L20 { get; set; }
        public int L21 { get; set; }
        public int L22 { get; set; }
        public int L23 { get; set; }
        public int L24 { get; set; }
        public int L25 { get; set; }
        public int L26 { get; set; }
        public int L27 { get; set; }
        public int L28 { get; set; }
        public int L29 { get; set; }
        public int L30 { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        public DateTime? DateUpdated { get; set; }
    }
}

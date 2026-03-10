// BackOfficeBlazor.Admin/Entities/ProductLevel.cs
using System.ComponentModel.DataAnnotations;

namespace BackOfficeBlazor.Admin.Entities
{
    public class ProductLevel
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(10)]
        public string PartNumber { get; set; } = string.Empty;

        // Only a few locations shown – you can extend up to Min30/Max30/Replenish30
        public int Min01 { get; set; }
        public int Max01 { get; set; }
        public bool Replenish01 { get; set; }

        public int Min02 { get; set; }
        public int Max02 { get; set; }
        public bool Replenish02 { get; set; }

        public int Min03 { get; set; }
        public int Max03 { get; set; }
        public bool Replenish03 { get; set; }

        // … continue as needed up to Min30/Max30/Replenish30
    }
}

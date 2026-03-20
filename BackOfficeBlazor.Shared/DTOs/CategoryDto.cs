using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class CategoryDto
    {
        [Key]
        [StringLength(10)]
        public string? Code { get; set; }

        [Required]
        [StringLength(200)]
        public string? Name { get; set; }
       
        public bool A { get; set; }
        public bool B { get; set; }
        public bool C { get; set; }

        public bool Major { get; set; }
        public bool IsDeleted { get; set; } = false;

        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime? DateUpdated { get; set; }
    }
}

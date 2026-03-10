using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class ManufacturerDto
    {
        [Key]
        [StringLength(4)]
        public string Code { get; set; }

        [Required]
        [StringLength(30)]
        public string Name { get; set; }

        public bool IsDeleted { get; set; }
     
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
}

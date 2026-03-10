using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class LocationDto
    {
        [StringLength(2)]
        public string Code { get; set; } = string.Empty;

        [StringLength(30)]
        public string Name { get; set; } = string.Empty;
    }
}

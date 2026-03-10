using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Shared.DTOs
{
    public class GroupProductDto
    {
        public int? GroupId { get; set; }

        public string GroupCode { get; set; } = string.Empty;

        public string GroupName { get; set; } = string.Empty;

        // Variants (child products)
        public List<ProductDto> Variants { get; set; } = new();
    }

}

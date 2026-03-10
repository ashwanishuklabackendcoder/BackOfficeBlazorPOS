using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Entities
{
    public class ProductGroup
    {
        [Key]
        public int GroupId { get; set; }

        public string GroupCode { get; set; } = null!;

        public string GroupName { get; set; } = null!;

        public DateTime CreatedOn { get; set; }
    }

}

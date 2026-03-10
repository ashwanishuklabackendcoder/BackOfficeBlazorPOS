using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Entities
{
    public class ProductGroupItem
    {
        [Key]
        public int Id { get; set; }
        public int GroupId { get; set; }

        public string PartNumber { get; set; } = null!;
    }

}

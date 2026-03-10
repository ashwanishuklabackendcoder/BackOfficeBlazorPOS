using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackOfficeBlazor.Admin.Entities
{
    public class StaffUserPermission
    {
        [Key, ForeignKey(nameof(User))]
        public int StaffUserId { get; set; }

        public bool Till { get; set; }
        public bool Workshop { get; set; }
        public bool ProductAdd { get; set; }
        public bool StockInput { get; set; }
        public bool Layaway { get; set; }
        public bool CustomerSalesReturn { get; set; }
        public bool Category { get; set; }
        public bool Brand { get; set; }
        public bool Supplier { get; set; }
        public bool Customer { get; set; }

        public StaffUser? User { get; set; }
    }
}

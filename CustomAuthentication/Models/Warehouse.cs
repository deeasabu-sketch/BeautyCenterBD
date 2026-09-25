using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CustomAuthentication.Models
{
    public class Warehouse
    {
        [Key]
        public int WarehouseId { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        [StringLength(30)]
        public string Code { get; set; }

        [StringLength(300)]
        public string Address { get; set; }

        [StringLength(30)]
        public string Phone { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Purchase> Purchases { get; set; }
        public virtual ICollection<Sale> Sales { get; set; }
        public virtual ICollection<StockTransaction> StockTransactions { get; set; }
    }
}
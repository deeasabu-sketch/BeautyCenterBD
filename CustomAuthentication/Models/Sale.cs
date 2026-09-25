using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CustomAuthentication.Models
{
    public class Sale
    {
        [Key]
        public int SaleId { get; set; }

        [Required, StringLength(50)]
        public string SaleNumber { get; set; }

        // Nullable: walk-in sale may have no Customer record.
        public int? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        [Required]
        public int WarehouseId { get; set; }
        [ForeignKey("WarehouseId")]
        public virtual Warehouse Warehouse { get; set; }

        [Required]
        public DateTime SaleDate { get; set; } = DateTime.Now;

        [Required]
        public decimal SubTotal { get; set; }

        public decimal Discount { get; set; }

        public decimal Tax { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required, StringLength(30)]
        public string Status { get; set; } = "Completed"; // Completed, Cancelled

        // Which logged-in User (typically SalesPerson) recorded this sale.
        public int CreatedByUserId { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<SaleDetail> SaleDetails { get; set; }
    }
}
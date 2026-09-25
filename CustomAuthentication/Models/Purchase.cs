using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CustomAuthentication.Models
{
    public class Purchase
    {
        [Key]
        public int PurchaseId { get; set; }

        [Required, StringLength(50)]
        public string PurchaseNumber { get; set; }

        [Required]
        public int SupplierId { get; set; }
        [ForeignKey("SupplierId")]
        public virtual Supplier Supplier { get; set; }

        [Required]
        public int WarehouseId { get; set; }
        [ForeignKey("WarehouseId")]
        public virtual Warehouse Warehouse { get; set; }

        [Required]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [Required]
        public decimal SubTotal { get; set; }

        public decimal Discount { get; set; }

        public decimal Tax { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required, StringLength(30)]
        public string Status { get; set; } = "Pending"; // Pending, Received, Cancelled

        // Which logged-in User recorded this purchase.
        public int CreatedByUserId { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<PurchaseDetail> PurchaseDetails { get; set; }
    }
}
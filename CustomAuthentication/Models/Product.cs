
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomAuthentication.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [StringLength(150)]
        public string ProductName { get; set; }

        [StringLength(1500)]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal UnitPrice { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        [StringLength(300)]
        public string ImagePath { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string SKU { get; set; }

        [Required]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        
        public int? BrandId { get; set; }
        [ForeignKey("BrandId")]
        public virtual Brand Brand { get; set; }

        
        public int? UnitId { get; set; }
        [ForeignKey("UnitId")]
        public virtual Unit Unit { get; set; }

        [Required]
        public decimal PurchasePrice { get; set; }

        [Required]
       

        // Percentage discount shown on the public shop (0 = no discount).
        [Range(0, 100)]
        public decimal DiscountPercent { get; set; } = 0;

        // Overall stock quantity across all warehouses. Per-warehouse
        // stock is tracked via StockTransaction records.
        

        public int ReorderLevel { get; set; } = 0;
        

      

        public virtual ICollection<PurchaseDetail> PurchaseDetails { get; set; }
        public virtual ICollection<SaleDetail> SaleDetails { get; set; }
        public virtual ICollection<StockTransaction> StockTransactions { get; set; }
        public virtual ICollection<StockTransferDetail> StockTransferDetails { get; set; }
        public virtual ICollection<ProductImage> ProductImages { get; set; }

        [NotMapped]
        public decimal DiscountedPrice => System.Math.Round(UnitPrice * (1 - DiscountPercent / 100m), 2);



    }
}


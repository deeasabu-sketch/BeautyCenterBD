using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CustomAuthentication.Models
{
    public class StockTransaction
    {
        [Key]
        public int StockTransactionId { get; set; }

        [Required]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [Required]
        public int WarehouseId { get; set; }
        [ForeignKey("WarehouseId")]
        public virtual Warehouse Warehouse { get; set; }

        [Required, StringLength(20)]
        public string TransactionType { get; set; } // "Purchase", "Sale", "TransferIn", "TransferOut", "Adjustment"

        // Positive for stock in, negative for stock out.
        [Required]
        public int QuantityChange { get; set; }

        public int? ReferenceId { get; set; } // PurchaseId / SaleId / StockTransferId

        [StringLength(50)]
        public string ReferenceType { get; set; } // "Purchase" / "Sale" / "StockTransfer"

        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        public int CreatedByUserId { get; set; }
    }
}
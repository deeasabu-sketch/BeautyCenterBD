using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CustomAuthentication.Models
{
    public class StockTransferDetail
    {
        [Key]
        public int StockTransferDetailId { get; set; }

        [Required]
        public int StockTransferId { get; set; }
        [ForeignKey("StockTransferId")]
        public virtual StockTransfer StockTransfer { get; set; }

        [Required]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [Required]
        public int Quantity { get; set; }
    }
}
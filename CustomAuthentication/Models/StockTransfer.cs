using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CustomAuthentication.Models
{
    public class StockTransfer
    {
        [Key]
        public int StockTransferId { get; set; }

        [Required, StringLength(50)]
        public string TransferNumber { get; set; }

        [Required]
        public int FromWarehouseId { get; set; }
        [ForeignKey("FromWarehouseId")]
        public virtual Warehouse FromWarehouse { get; set; }

        [Required]
        public int ToWarehouseId { get; set; }
        [ForeignKey("ToWarehouseId")]
        public virtual Warehouse ToWarehouse { get; set; }

        [Required]
        public DateTime TransferDate { get; set; } = DateTime.Now;

        // Requested -> Approved/Rejected -> Sent -> Received
        // (Cancelled possible before Sent)
        [Required, StringLength(30)]
        public string Status { get; set; } = "Requested";

        // Step 1: BranchManager who requested the transfer.
        public int RequestedByUserId { get; set; }
        public DateTime? RequestedDate { get; set; }

        // Step 2: StoreManager (source side) who approved/rejected.
        public int? ApprovedByUserId { get; set; }
        public DateTime? ApprovedDate { get; set; }

        [StringLength(300)]
        public string RejectionReason { get; set; }

        // Step 3: source-warehouse WarehouseManager who marked it Sent.
        public int? SentByUserId { get; set; }
        public DateTime? SentDate { get; set; }

        // Step 4: destination-warehouse WarehouseManager who confirmed Received.
        public int? ReceivedByUserId { get; set; }
        public DateTime? ReceivedDate { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<StockTransferDetail> StockTransferDetails { get; set; }
    }
}
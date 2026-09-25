using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomAuthentication.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, StringLength(30)]
        public string Phone { get; set; }

        [StringLength(500)]
        public string Address { get; set; }

        [Required, StringLength(100), EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string RoleName { get; set; }

        [StringLength(300)]
        public string ImagePath { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.Now;





        // Null for Admin / CentralManager (they are not tied to one
        // warehouse). Required in practice for BranchManager,
        // StoreManager, SalesPerson, WarehouseManager.
        public int? WarehouseId { get; set; }

        [ForeignKey("WarehouseId")]
        public virtual Warehouse Warehouse { get; set; }

        [StringLength(300)]
        public string ProfileImagePath { get; set; }

        // =========================
        // ADMIN INVITATION / PASSWORD SETUP
        // =========================
        [StringLength(128)]
        public string PasswordSetupTokenHash { get; set; }

        public DateTime? PasswordSetupExpiresAt { get; set; }
        public DateTime? PasswordSetupCompletedAt { get; set; }
        public DateTime? InvitationSentAt { get; set; }
        public DateTime? InvitationReminderSentAt { get; set; }


        // =========================
        // USER PERMISSIONS
        // =========================

        public bool CanViewDashboard { get; set; } = true;

        public bool CanViewProducts { get; set; } = false;
        public bool CanAddProducts { get; set; } = false;
        public bool CanEditProducts { get; set; } = false;
        public bool CanDeleteProducts { get; set; } = false;

        public bool CanViewOrders { get; set; } = false;
        public bool CanAddOrders { get; set; } = false;
        public bool CanEditOrders { get; set; } = false;
        public bool CanDeleteOrders { get; set; } = false;

        public bool CanViewReports { get; set; } = false;

        public bool CanManageUsers { get; set; } = false;

        public bool CanViewNotebook { get; set; } = false;
        public bool CanAddNotebook { get; set; } = false;
        public bool CanEditNotebook { get; set; } = false;
        public bool CanDeleteNotebook { get; set; } = false;
        public bool CanManagePromoCodes { get; set; } = false;
    }
}
using System;
using System.ComponentModel.DataAnnotations;

namespace CustomAuthentication.ViewModel
{
    // Read-only summary of a staff/admin account, used by
    // Admin/Users (a quick overview list). Full create/edit
    // still goes through UserController + UserManagementViewModel.
    public class AdminUserViewModel
    {
        public int UserId { get; set; }
        public string Phone { get; set; }
        public string WarehouseName { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        [StringLength(100)]
        public string Email { get; set; }

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string RoleName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public DateTime CreateDate { get; set; }


        // =========================
        // PERMISSIONS
        // =========================

        public bool CanViewDashboard { get; set; }

        public bool CanViewProducts { get; set; }
        public bool CanAddProducts { get; set; }
        public bool CanEditProducts { get; set; }
        public bool CanDeleteProducts { get; set; }

        public bool CanViewOrders { get; set; }
        public bool CanAddOrders { get; set; }
        public bool CanEditOrders { get; set; }
        public bool CanDeleteOrders { get; set; }

        public bool CanViewReports { get; set; }

        public bool CanManageUsers { get; set; }

        public bool CanViewNotebook { get; set; }
        public bool CanAddNotebook { get; set; }
        public bool CanEditNotebook { get; set; }
        public bool CanDeleteNotebook { get; set; }
    }
}

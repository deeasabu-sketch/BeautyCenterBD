using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CustomAuthentication.ViewModel
{
    public class UserManagementViewModel
    {
        public int UserId { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, StringLength(100), EmailAddress]
        public string Email { get; set; }

        [Required, StringLength(30)]
        public string Phone { get; set; }

        // Password is set by the invited user from the secure email link.
        // Kept only for backward compatibility with older Edit screens.
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string RoleName { get; set; }

        // Required for warehouse-scoped roles, ignored for Admin/HR/CentralManager.
        public int? WarehouseId { get; set; }

        public bool IsActive { get; set; } = true;

        public IEnumerable<SelectListItem> RoleOptions { get; set; }
        public IEnumerable<SelectListItem> WarehouseOptions { get; set; }

        // =========================
        // PER-USER PERMISSIONS
        // These override the role-based matrix for THIS user only.
        // =========================

        public bool CanViewDashboard { get; set; } = true;

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
        public bool CanManagePromoCodes { get; set; }
    }
}
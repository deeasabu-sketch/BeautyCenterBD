using CustomAuthentication.Data;
using CustomAuthentication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomAuthentication.Service
{
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext db;

        public PermissionService(AppDbContext context)
        {
            db = context;
        }

        private RolePermission GetRolePermission(string roleName, string moduleName)
        {
            return db.RolePermissions
                .FirstOrDefault(rp => rp.RoleName == roleName && rp.Module.Name == moduleName);
        }

        // =====================================================
        // ROLE-BASED CHECKS (original behaviour, unchanged).
        // Used when we only have a role name, no specific user.
        // =====================================================

        public bool CanView(string roleName, string moduleName)
        {
            if (roleName == UserRoles.Admin) return true;
            var rp = GetRolePermission(roleName, moduleName);
            return rp != null && rp.CanView;
        }

        public bool CanCreate(string roleName, string moduleName)
        {
            if (roleName == UserRoles.Admin) return true;
            var rp = GetRolePermission(roleName, moduleName);
            return rp != null && rp.CanCreate;
        }

        public bool CanEdit(string roleName, string moduleName)
        {
            if (roleName == UserRoles.Admin) return true;
            var rp = GetRolePermission(roleName, moduleName);
            return rp != null && rp.CanEdit;
        }

        public bool CanDelete(string roleName, string moduleName)
        {
            if (roleName == UserRoles.Admin) return true;
            var rp = GetRolePermission(roleName, moduleName);
            return rp != null && rp.CanDelete;
        }

        // =====================================================
        // PER-USER OVERRIDE CHECKS
        // For modules where the User table has its own explicit
        // Can... flags (Products, Orders, Notebook, Reports,
        // Dashboard, Users), that per-user flag is the final answer
        // for THAT user - it overrides whatever the role matrix says.
        // For every other module, we just fall back to the role matrix.
        // =====================================================

        private enum Action { View, Create, Edit, Delete }

        private bool? GetUserOverride(User user, string moduleName, Action action)
        {
            if (user == null) return null;

            switch (moduleName)
            {
                case "Products":
                    switch (action)
                    {
                        case Action.View: return user.CanViewProducts;
                        case Action.Create: return user.CanAddProducts;
                        case Action.Edit: return user.CanEditProducts;
                        case Action.Delete: return user.CanDeleteProducts;
                    }
                    break;

                case "Orders":
                    switch (action)
                    {
                        case Action.View: return user.CanViewOrders;
                        case Action.Create: return user.CanAddOrders;
                        case Action.Edit: return user.CanEditOrders;
                        case Action.Delete: return user.CanDeleteOrders;
                    }
                    break;

                case "Notebook":
                    switch (action)
                    {
                        case Action.View: return user.CanViewNotebook;
                        case Action.Create: return user.CanAddNotebook;
                        case Action.Edit: return user.CanEditNotebook;
                        case Action.Delete: return user.CanDeleteNotebook;
                    }
                    break;

                case "Reports":
                    if (action == Action.View) return user.CanViewReports;
                    break;

                case "Dashboard":
                    if (action == Action.View) return user.CanViewDashboard;
                    break;

                case "Users":
                    // one flag covers all actions for this module
                    return user.CanManageUsers;
                case "PromoCodes":
                    return user.CanManagePromoCodes;
            }

            return null; // no per-user override for this module/action -> use role matrix
        }

        public bool CanView(User user, string moduleName)
        {
            if (user == null) return false;
            if (user.RoleName == UserRoles.Admin) return true;

            var overrideValue = GetUserOverride(user, moduleName, Action.View);
            if (overrideValue.HasValue) return overrideValue.Value;

            return CanView(user.RoleName, moduleName);
        }

        public bool CanCreate(User user, string moduleName)
        {
            if (user == null) return false;
            if (user.RoleName == UserRoles.Admin) return true;

            var overrideValue = GetUserOverride(user, moduleName, Action.Create);
            if (overrideValue.HasValue) return overrideValue.Value;

            return CanCreate(user.RoleName, moduleName);
        }

        public bool CanEdit(User user, string moduleName)
        {
            if (user == null) return false;
            if (user.RoleName == UserRoles.Admin) return true;

            var overrideValue = GetUserOverride(user, moduleName, Action.Edit);
            if (overrideValue.HasValue) return overrideValue.Value;

            return CanEdit(user.RoleName, moduleName);
        }

        public bool CanDelete(User user, string moduleName)
        {
            if (user == null) return false;
            if (user.RoleName == UserRoles.Admin) return true;

            var overrideValue = GetUserOverride(user, moduleName, Action.Delete);
            if (overrideValue.HasValue) return overrideValue.Value;

            return CanDelete(user.RoleName, moduleName);
        }
    }
}
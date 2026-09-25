using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomAuthentication.Models
{
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Employee = "Employee";
        public const string Customer = "Customer";
        public const string Accountant = "Accountant";
       
        public const string HR = "HR";
        public const string CentralManager = "CentralManager";
        public const string BranchManager = "BranchManager";
        public const string StoreManager = "StoreManager";
        public const string SalesPerson = "SalesPerson";
        public const string WarehouseManager = "WarehouseManager";

        // Legacy roles kept for the existing Order/ecommerce module.
       

        public static readonly string[] AllInventoryRoles = new[]
        {
            Admin,
            HR,
            CentralManager,
            BranchManager,
            StoreManager,
            SalesPerson,
            WarehouseManager
        };

        // Roles whose data access is scoped to a single Warehouse
        // (via User.WarehouseId). Admin, HR and CentralManager are not
        // warehouse-scoped.
        public static readonly string[] WarehouseScopedRoles = new[]
        {
            BranchManager,
            StoreManager,
            SalesPerson,
            WarehouseManager
        };

        // Roles allowed to manage Users (invite/edit/deactivate/assign role).
        public static readonly string[] UserManagementRoles = new[]
        {
            Admin,
            HR
        };

    }
}
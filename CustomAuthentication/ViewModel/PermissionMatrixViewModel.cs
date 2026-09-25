using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomAuthentication.ViewModel
{
    public class PermissionMatrixViewModel
    {
        public List<string> Roles { get; set; }
        public List<RolePermissionRow> Rows { get; set; }
    }

    // One row per (Role, Module) - matches a RolePermission record.
    public class RolePermissionRow
    {
        public int RolePermissionId { get; set; }
        public string RoleName { get; set; }
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
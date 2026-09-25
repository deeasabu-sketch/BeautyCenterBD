using CustomAuthentication.Filters;
using CustomAuthentication.Models;
using CustomAuthentication.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    // Admin/HR-only: lets them decide what each inventory role (HR,
    // CentralManager, BranchManager, StoreManager, SalesPerson,
    // WarehouseManager) can view/create/edit/delete per module.
    // Admin itself always has full access and is not shown as an
    // editable row (see PermissionService.CanView/Create/Edit/Delete).
    public class PermissionController : InventoryBaseController
    {
        private static readonly string[] EditableRoles =
        {
            UserRoles.HR,
            UserRoles.CentralManager,
            UserRoles.BranchManager,
            UserRoles.StoreManager,
            UserRoles.SalesPerson,
            UserRoles.WarehouseManager
        };

        [RequirePermission("Users", PermissionAction.View)]
        public ActionResult Index()
        {
            var modules = db.Modules
                .OrderBy(m => m.SortOrder)
                .ToList();

            var existingPermissions = db.RolePermissions.ToList();

            var rows = new List<RolePermissionRow>();

            foreach (var role in EditableRoles)
            {
                foreach (var module in modules)
                {
                    var existing = existingPermissions.FirstOrDefault(
                        rp => rp.RoleName == role && rp.ModuleId == module.ModuleId);

                    rows.Add(new RolePermissionRow
                    {
                        RolePermissionId = existing?.RolePermissionId ?? 0,
                        RoleName = role,
                        ModuleId = module.ModuleId,
                        ModuleName = module.Name,
                        CanView = existing?.CanView ?? false,
                        CanCreate = existing?.CanCreate ?? false,
                        CanEdit = existing?.CanEdit ?? false,
                        CanDelete = existing?.CanDelete ?? false
                    });
                }
            }

            var model = new PermissionMatrixViewModel
            {
                Roles = EditableRoles.ToList(),
                Rows = rows
            };

            return View(model);
        }

        [RequirePermission("Users", PermissionAction.Edit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(List<RolePermissionRow> Rows)
        {
            if (Rows == null)
            {
                TempData["Error"] = "No changes were submitted.";
                return RedirectToAction("Index");
            }

            foreach (var row in Rows)
            {
                var existing = db.RolePermissions.FirstOrDefault(
                    rp => rp.RoleName == row.RoleName && rp.ModuleId == row.ModuleId);

                if (existing == null)
                {
                    db.RolePermissions.Add(new RolePermission
                    {
                        RoleName = row.RoleName,
                        ModuleId = row.ModuleId,
                        CanView = row.CanView,
                        CanCreate = row.CanCreate,
                        CanEdit = row.CanEdit,
                        CanDelete = row.CanDelete
                    });
                }
                else
                {
                    existing.CanView = row.CanView;
                    existing.CanCreate = row.CanCreate;
                    existing.CanEdit = row.CanEdit;
                    existing.CanDelete = row.CanDelete;
                }
            }

            db.SaveChanges();

            TempData["Success"] = "Permissions updated successfully.";
            return RedirectToAction("Index");
        }
    }
}

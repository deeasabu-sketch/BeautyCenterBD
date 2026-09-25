using CustomAuthentication.Data;
using CustomAuthentication.Models;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    // Shared helpers for every inventory-module controller (Category,
    // Brand, Unit, Supplier, Customer, Warehouse, Purchase, Sale,
    // StockTransfer, Salary, UserManagement, Permission): current user,
    // current role, current warehouse scope. Inherit this instead of
    // Controller directly.
    public abstract class InventoryBaseController : Controller
    {
        protected readonly AppDbContext db = new AppDbContext();

        // The logged-in User row (Email is stored as the FormsAuth identity name).
        protected User CurrentUser
        {
            get
            {
                if (User == null || User.Identity == null || !User.Identity.IsAuthenticated)
                {
                    return null;
                }

                return db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            }
        }

        protected string CurrentRole
        {
            get { return CurrentUser?.RoleName; }
        }

        protected int? CurrentWarehouseId
        {
            get { return CurrentUser?.WarehouseId; }
        }

        // True for Admin/HR/CentralManager - roles that are NOT limited
        // to a single warehouse.
        protected bool IsUnrestricted
        {
            get
            {
                var role = CurrentRole;
                return role == UserRoles.Admin
                    || role == UserRoles.HR
                    || role == UserRoles.CentralManager;
            }
        }

        // Apply to any IQueryable<T> that has a WarehouseId column, so
        // warehouse-scoped roles (BranchManager, StoreManager,
        // SalesPerson, WarehouseManager) only see their own warehouse's rows.
        protected IQueryable<T> ScopeToWarehouse<T>(
            IQueryable<T> query,
            System.Linq.Expressions.Expression<System.Func<T, int>> warehouseIdSelector)
        {
            if (IsUnrestricted || CurrentWarehouseId == null)
            {
                return query;
            }

            var param = warehouseIdSelector.Parameters[0];

            var body = System.Linq.Expressions.Expression.Equal(
                warehouseIdSelector.Body,
                System.Linq.Expressions.Expression.Constant(CurrentWarehouseId.Value));

            var lambda = System.Linq.Expressions.Expression.Lambda<System.Func<T, bool>>(body, param);

            return query.Where(lambda);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

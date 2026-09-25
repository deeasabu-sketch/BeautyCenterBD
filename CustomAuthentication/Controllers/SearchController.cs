using CustomAuthentication.Data;
using CustomAuthentication.Helpers;
using CustomAuthentication.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    // =========================================
    // GLOBAL SEARCH (Products / Orders / Users)
    // =========================================
    // Used by the AJAX search box on Home/Index
    // and Admin/Dashboard. Results are filtered
    // per logged-in user's role & permissions so
    // everyone only sees what they're allowed to.
    public class SearchController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        private bool HasPermission(string permission)
        {
            if (User == null || !User.Identity.IsAuthenticated)
            {
                return false;
            }

            return PermissionHelper.HasPermission(db, User, permission);
        }

        // GET: /Search/Global?q=...
        [HttpGet]
        public JsonResult Global(string q)
        {
            var result = new
            {
                success = true,
                query = q,
                products = new object[0],
                orders = new object[0],
                users = new object[0]
            };

            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            string term = q.Trim();

            try
            {
                bool isAuthenticated = User != null && User.Identity.IsAuthenticated;
            bool isAdmin = isAuthenticated && User.IsInRole("Admin");

            // =========================
            // PRODUCTS (public - anyone browsing the shop can search)
            // =========================
            object[] products = db.Products
                .Include(p => p.Category)
                .Where(p =>
                    p.IsActive &&
                    (p.ProductName.Contains(term) ||
                    (p.Category != null && p.Category.Name.Contains(term)) ||
                    (p.SKU != null && p.SKU.Contains(term)) ||
                    (p.Description != null && p.Description.Contains(term))))
                .OrderBy(p => p.ProductName)
                .Take(8)
                .ToList()
                .Select(p => new
                {
                    id = p.ProductId,
                    title = p.ProductName,
                    subtitle = (p.Category != null ? p.Category.Name : "Uncategorized") + " • Taka " + p.UnitPrice.ToString("N2"),
                    badge = p.IsActive ? "In Stock" : "Unavailable",
                    url = Url.Action("Details", "Product", new { id = p.ProductId })
                })
                .Cast<object>()
                .ToArray();

            // =========================
            // ORDERS
            // =========================
            object[] orders = new object[0];

            bool canViewOrders = isAdmin || HasPermission("CanViewOrders");

            if (canViewOrders)
            {
                orders = db.Orders
                    .Where(o =>
                        o.OrderNumber.Contains(term) ||
                        (o.OrderStatus != null && o.OrderStatus.Contains(term)) ||
                        (o.CustomerPhone != null && o.CustomerPhone.Contains(term)) ||
                        (o.CustomerEmail != null && o.CustomerEmail.Contains(term)))
                    .OrderByDescending(o => o.OrderDate)
                    .Take(8)
                    .ToList()
                    .Select(o => new
                    {
                        id = o.OrderId,
                        title = o.OrderNumber,
                        subtitle = o.OrderStatus + " • Taka " + o.TotalAmount.ToString("N2") + " • " + o.OrderDate.ToString("dd MMM yyyy"),
                        badge = o.OrderStatus,
                        url = Url.Action("OrderDetails", "Admin", new { id = o.OrderId })
                    })
                    .Cast<object>()
                    .ToArray();
            }

            // =========================
            // USERS (Admin/HR only)
            // =========================
            object[] users = new object[0];

            string currentRoleName = isAuthenticated
                ? db.Users.FirstOrDefault(u => u.Email == User.Identity.Name)?.RoleName
                : null;

            bool canManageUsers = currentRoleName != null &&
                UserRoles.UserManagementRoles.Contains(currentRoleName);

            if (canManageUsers)
            {
                users = db.Users
                    .Where(u =>
                        u.FullName.Contains(term) ||
                        u.Email.Contains(term) ||
                        (u.Phone != null && u.Phone.Contains(term)) ||
                        u.RoleName.Contains(term))
                    .OrderBy(u => u.FullName)
                    .Take(8)
                    .ToList()
                    .Select(u => new
                    {
                        id = u.UserId,
                        title = u.FullName,
                        subtitle = u.Email + " • " + u.RoleName,
                        badge = u.IsActive ? "Active" : "Inactive",
                        url = Url.Action("Index", "User", new { id = u.UserId })
                    })
                    .Cast<object>()
                    .ToArray();
            }

                var finalResult = new
                {
                    success = true,
                    query = term,
                    products = products,
                    orders = orders,
                    users = users,
                    totalCount = products.Length + orders.Length + users.Length
                };

                return Json(finalResult, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("SearchController.Global failed: " + ex);
                return Json(new
                {
                    success = false,
                    query = term,
                    products = new object[0],
                    orders = new object[0],
                    users = new object[0],
                    totalCount = 0,
                    message = "Search is temporarily unavailable."
                }, JsonRequestBehavior.AllowGet);
            }
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

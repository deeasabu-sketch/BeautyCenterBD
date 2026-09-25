using CustomAuthentication.Data;
using CustomAuthentication.Helpers;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    // =========================================================
    // ANALYTICS - JSON endpoints that feed the small "Graph"
    // buttons on the Dashboard and on the Product / Order list
    // pages. Every action re-checks the same User.CanX permission
    // that gates the matching Dashboard section, so a user can
    // never pull numbers for something they aren't allowed to see
    // just by knowing the URL.
    // =========================================================
    [Authorize]
    public class AnalyticsController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        private bool HasPermission(string permission)
        {
            return PermissionHelper.HasPermission(db, User, permission);
        }

        // Last N months, oldest first, as (label, year, month) tuples.
        private System.Collections.Generic.List<(string Label, int Year, int Month)> LastMonths(int count)
        {
            var result = new System.Collections.Generic.List<(string, int, int)>();
            var cursor = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            for (int i = count - 1; i >= 0; i--)
            {
                var d = cursor.AddMonths(-i);
                result.Add((d.ToString("MMM yyyy"), d.Year, d.Month));
            }

            return result;
        }

        // GET: /Analytics/Orders?months=6
        [HttpGet]
        public JsonResult Orders(int months = 6)
        {
            if (!HasPermission("CanViewOrders"))
            {
                return Json(new { success = false, message = "You do not have permission to view order analytics." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var buckets = LastMonths(months);
                var earliestStart = new DateTime(buckets[0].Year, buckets[0].Month, 1);

                var orders = db.Orders
                    .Where(o => o.IsActive && o.OrderDate >= earliestStart)
                    .Select(o => new { o.OrderDate, o.TotalAmount })
                    .ToList();

                var labels = buckets.Select(b => b.Label).ToArray();

                var orderCounts = buckets
                    .Select(b => orders.Count(o => o.OrderDate.Year == b.Year && o.OrderDate.Month == b.Month))
                    .ToArray();

                var orderAmounts = buckets
                    .Select(b => orders.Where(o => o.OrderDate.Year == b.Year && o.OrderDate.Month == b.Month).Sum(o => o.TotalAmount))
                    .ToArray();

                return Json(new
                {
                    success = true,
                    labels,
                    orderCounts,
                    orderAmounts,
                    thisMonthCount = orderCounts.LastOrDefault(),
                    thisMonthAmount = orderAmounts.LastOrDefault()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("AnalyticsController.Orders failed: " + ex);
                return Json(new { success = false, message = "Could not load order analytics." }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Analytics/Products?months=6
        // "Products added per month" + current active/inactive split.
        [HttpGet]
        public JsonResult Products(int months = 6)
        {
            if (!HasPermission("CanViewProducts"))
            {
                return Json(new { success = false, message = "You do not have permission to view product analytics." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var buckets = LastMonths(months);
                var earliestStart = new DateTime(buckets[0].Year, buckets[0].Month, 1);

                var products = db.Products
                    .Where(p => p.CreateDate >= earliestStart)
                    .Select(p => new { p.CreateDate })
                    .ToList();

                var labels = buckets.Select(b => b.Label).ToArray();

                var addedCounts = buckets
                    .Select(b => products.Count(p => p.CreateDate.Year == b.Year && p.CreateDate.Month == b.Month))
                    .ToArray();

                return Json(new
                {
                    success = true,
                    labels,
                    addedCounts,
                    totalProducts = db.Products.Count(),
                    activeProducts = db.Products.Count(p => p.IsActive),
                    lowStockProducts = db.Products.Count(p => p.IsActive && p.StockQuantity > 0 && p.StockQuantity <= 5),
                    outOfStockProducts = db.Products.Count(p => p.IsActive && p.StockQuantity <= 0),
                    totalStockUnits = db.Products.Where(p => p.IsActive).Select(p => (int?)p.StockQuantity).Sum() ?? 0,
                    thisMonthAdded = addedCounts.LastOrDefault()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("AnalyticsController.Products failed: " + ex);
                return Json(new { success = false, message = "Could not load product analytics." }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Analytics/Users
        // User counts by designation/role plus active/inactive totals.
        [HttpGet]
        public JsonResult Users()
        {
            if (!HasPermission("CanManageUsers"))
            {
                return Json(new { success = false, message = "You do not have permission to view user analytics." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var roleGroups = db.Users
                    .Where(u => u.RoleName != Models.UserRoles.Customer)
                    .GroupBy(u => u.RoleName)
                    .Select(g => new { Role = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                return Json(new
                {
                    success = true,
                    labels = roleGroups.Select(x => x.Role).ToArray(),
                    roleCounts = roleGroups.Select(x => x.Count).ToArray(),
                    totalUsers = db.Users.Count(u => u.RoleName != Models.UserRoles.Customer),
                    activeUsers = db.Users.Count(u => u.RoleName != Models.UserRoles.Customer && u.IsActive),
                    inactiveUsers = db.Users.Count(u => u.RoleName != Models.UserRoles.Customer && !u.IsActive)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("AnalyticsController.Users failed: " + ex);
                return Json(new { success = false, message = "Could not load user analytics." }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Analytics/Purchases?months=6
        [HttpGet]
        public JsonResult Purchases(int months = 6)
        {
            bool isAdmin = User.IsInRole(Models.UserRoles.Admin);
            if (!isAdmin && !HasPermission("CanViewReports"))
            {
                return Json(new { success = false, message = "You do not have permission to view purchase analytics." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var buckets = LastMonths(months);
                var earliestStart = new DateTime(buckets[0].Year, buckets[0].Month, 1);

                var purchases = db.Purchases
                    .Where(p => p.IsActive && p.Status == "Received" && p.PurchaseDate >= earliestStart)
                    .Select(p => new { p.PurchaseDate, p.TotalAmount })
                    .ToList();

                var labels = buckets.Select(b => b.Label).ToArray();

                var purchaseCounts = buckets
                    .Select(b => purchases.Count(p => p.PurchaseDate.Year == b.Year && p.PurchaseDate.Month == b.Month))
                    .ToArray();

                var purchaseAmounts = buckets
                    .Select(b => purchases.Where(p => p.PurchaseDate.Year == b.Year && p.PurchaseDate.Month == b.Month).Sum(p => p.TotalAmount))
                    .ToArray();

                return Json(new
                {
                    success = true,
                    labels,
                    purchaseCounts,
                    purchaseAmounts
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("AnalyticsController.Purchases failed: " + ex);
                return Json(new { success = false, message = "Could not load purchase analytics." }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Analytics/Sales?months=6
        [HttpGet]
        public JsonResult Sales(int months = 6)
        {
            bool isAdmin = User.IsInRole(Models.UserRoles.Admin);
            if (!isAdmin && !HasPermission("CanViewReports"))
            {
                return Json(new { success = false, message = "You do not have permission to view sale analytics." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var buckets = LastMonths(months);
                var earliestStart = new DateTime(buckets[0].Year, buckets[0].Month, 1);

                var sales = db.Sales
                    .Where(s => s.IsActive && s.Status == "Completed" && s.SaleDate >= earliestStart)
                    .Select(s => new { s.SaleDate, s.TotalAmount })
                    .ToList();

                var labels = buckets.Select(b => b.Label).ToArray();

                var saleCounts = buckets
                    .Select(b => sales.Count(s => s.SaleDate.Year == b.Year && s.SaleDate.Month == b.Month))
                    .ToArray();

                var saleAmounts = buckets
                    .Select(b => sales.Where(s => s.SaleDate.Year == b.Year && s.SaleDate.Month == b.Month).Sum(s => s.TotalAmount))
                    .ToArray();

                return Json(new
                {
                    success = true,
                    labels,
                    saleCounts,
                    saleAmounts
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("AnalyticsController.Sales failed: " + ex);
                return Json(new { success = false, message = "Could not load sale analytics." }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Analytics/PromoCodes
        [HttpGet]
        public JsonResult PromoCodes()
        {
            if (!HasPermission("CanManagePromoCodes"))
            {
                return Json(new { success = false, message = "You do not have permission to view promo code analytics." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                return Json(new
                {
                    success = true,
                    labels = new[] { "Active", "Inactive", "Expired" },
                    counts = new[]
                    {
                        db.PromoCodes.Count(p => p.IsActive && (!p.ExpiryDate.HasValue || p.ExpiryDate.Value >= DateTime.Now)),
                        db.PromoCodes.Count(p => !p.IsActive),
                        db.PromoCodes.Count(p => p.IsActive && p.ExpiryDate.HasValue && p.ExpiryDate.Value < DateTime.Now)
                    },
                    total = db.PromoCodes.Count()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("AnalyticsController.PromoCodes failed: " + ex);
                return Json(new { success = false, message = "Could not load promo code analytics." }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Analytics/Financials?months=6
        // Monthly sales, product purchases and paid salaries.
        [HttpGet]
        public JsonResult Financials(int months = 6)
        {
            bool isAdmin = User.IsInRole(Models.UserRoles.Admin);
            if (!isAdmin && !HasPermission("CanViewReports"))
            {
                return Json(new { success = false, message = "You do not have permission to view financial analytics." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var buckets = LastMonths(months);
                var earliestStart = new DateTime(buckets[0].Year, buckets[0].Month, 1);

                var sales = db.Sales
                    .Where(s => s.IsActive && s.Status == "Completed" && s.SaleDate >= earliestStart)
                    .Select(s => new { s.SaleDate, s.TotalAmount })
                    .ToList();

                var purchases = db.Purchases
                    .Where(p => p.IsActive && p.Status == "Received" && p.PurchaseDate >= earliestStart)
                    .Select(p => new { p.PurchaseDate, p.TotalAmount })
                    .ToList();

                var salaries = db.Salaries
                    .Where(x => x.IsActive && x.Status == "Paid" &&
                        (x.SalaryYear > earliestStart.Year ||
                         (x.SalaryYear == earliestStart.Year && x.SalaryMonth >= earliestStart.Month)))
                    .Select(x => new { x.SalaryYear, x.SalaryMonth, x.NetSalary })
                    .ToList();

                var labels = buckets.Select(b => b.Label).ToArray();
                var saleAmounts = buckets.Select(b => sales.Where(x => x.SaleDate.Year == b.Year && x.SaleDate.Month == b.Month).Sum(x => x.TotalAmount)).ToArray();
                var purchaseAmounts = buckets.Select(b => purchases.Where(x => x.PurchaseDate.Year == b.Year && x.PurchaseDate.Month == b.Month).Sum(x => x.TotalAmount)).ToArray();
                var salaryAmounts = buckets.Select(b => salaries.Where(x => x.SalaryYear == b.Year && x.SalaryMonth == b.Month).Sum(x => x.NetSalary)).ToArray();

                var currentMonth = buckets.Last();
                var thisMonthSaleCount = sales.Count(x => x.SaleDate.Year == currentMonth.Year && x.SaleDate.Month == currentMonth.Month);
                var thisMonthPurchaseCount = purchases.Count(x => x.PurchaseDate.Year == currentMonth.Year && x.PurchaseDate.Month == currentMonth.Month);
                var thisMonthOrders = db.Orders.Count(o => o.IsActive && o.OrderDate.Year == currentMonth.Year && o.OrderDate.Month == currentMonth.Month);

                return Json(new
                {
                    success = true, labels, saleAmounts, purchaseAmounts, salaryAmounts,
                    thisMonthSaleCount,
                    thisMonthPurchaseCount,
                    thisMonthOrders,
                    thisMonthSales = saleAmounts.LastOrDefault(),
                    thisMonthPurchases = purchaseAmounts.LastOrDefault(),
                    thisMonthSalary = salaryAmounts.LastOrDefault()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("AnalyticsController.Financials failed: " + ex);
                return Json(new { success = false, message = "Could not load financial analytics." }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Analytics/Warehouses
        // Per-warehouse stock-transaction / staff-count snapshot for the
        // Warehouse Management graph button.
        [HttpGet]
        public JsonResult Warehouses()
        {
            bool isAdmin = User.IsInRole(Models.UserRoles.Admin);
            if (!isAdmin && !HasPermission("CanViewReports"))
            {
                return Json(new { success = false, message = "You do not have permission to view warehouse analytics." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var warehouses = db.Warehouses
                    .Where(w => w.IsActive)
                    .Select(w => new
                    {
                        w.WarehouseId,
                        w.Name
                    })
                    .ToList();

                var labels = warehouses.Select(w => w.Name).ToArray();

                var staffCounts = warehouses
                    .Select(w => db.Users.Count(u => u.WarehouseId == w.WarehouseId && u.IsActive))
                    .ToArray();

                var purchaseCounts = warehouses
                    .Select(w => db.Purchases.Count(p => p.WarehouseId == w.WarehouseId && p.IsActive))
                    .ToArray();

                var saleCounts = warehouses
                    .Select(w => db.Sales.Count(s => s.WarehouseId == w.WarehouseId && s.IsActive))
                    .ToArray();

                return Json(new
                {
                    success = true,
                    labels,
                    staffCounts,
                    purchaseCounts,
                    saleCounts
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("AnalyticsController.Warehouses failed: " + ex);
                return Json(new { success = false, message = "Could not load warehouse analytics." }, JsonRequestBehavior.AllowGet);
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

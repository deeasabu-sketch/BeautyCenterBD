using CustomAuthentication.Data;
using CustomAuthentication.Helpers;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    [Authorize(Roles = "Accountant")]
    public class AccountantController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        private bool HasPermission(string permission)
        {
            return PermissionHelper.HasPermission(db, User, permission);
        }

        public ActionResult Dashboard()
        {
            var user = db.Users.FirstOrDefault(x => x.Email == User.Identity.Name);

            // =========================
            // MY ACCESS (permission flags for this logged-in user)
            // =========================
            ViewBag.CurrentUser = user;

            // =========================
            // ORDER / SALES STATS (only if permitted)
            // =========================
            if (HasPermission("CanViewOrders"))
            {
                DateTime todayStart = DateTime.Today;
                DateTime tomorrowStart = todayStart.AddDays(1);

                DateTime monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                DateTime nextMonthStart = monthStart.AddMonths(1);

                ViewBag.TodayOrders = db.Orders.Count(
                    x => x.OrderDate >= todayStart && x.OrderDate < tomorrowStart);

                ViewBag.TodayAmount = db.Orders
                    .Where(x => x.OrderDate >= todayStart && x.OrderDate < tomorrowStart)
                    .Sum(x => (decimal?)x.TotalAmount) ?? 0;

                ViewBag.MonthOrders = db.Orders.Count(
                    x => x.OrderDate >= monthStart && x.OrderDate < nextMonthStart);

                ViewBag.MonthAmount = db.Orders
                    .Where(x => x.OrderDate >= monthStart && x.OrderDate < nextMonthStart)
                    .Sum(x => (decimal?)x.TotalAmount) ?? 0;

                ViewBag.TotalOrders = db.Orders.Count();

                ViewBag.TotalAmount = db.Orders.Sum(x => (decimal?)x.TotalAmount) ?? 0;
            }

            // =========================
            // REPORTS ACCESS
            // =========================
            ViewBag.CanViewReports = HasPermission("CanViewReports");

            return View();
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

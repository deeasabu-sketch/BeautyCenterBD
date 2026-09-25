using CustomAuthentication.Data;
using CustomAuthentication.Helpers;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
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
            // PRODUCT STATS (only if permitted)
            // =========================
            if (HasPermission("CanViewProducts"))
            {
                ViewBag.TotalProducts = db.Products.Count();
                ViewBag.ActiveProducts = db.Products.Count(x => x.IsActive);
            }

            // =========================
            // ORDER STATS (only if permitted)
            // =========================
            if (HasPermission("CanViewOrders"))
            {
                DateTime todayStart = DateTime.Today;
                DateTime tomorrowStart = todayStart.AddDays(1);

                ViewBag.TodayOrders = db.Orders.Count(
                    x => x.OrderDate >= todayStart && x.OrderDate < tomorrowStart);

                ViewBag.TotalOrders = db.Orders.Count();
            }

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

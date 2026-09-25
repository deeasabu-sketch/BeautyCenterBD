using CustomAuthentication.Filters;
using CustomAuthentication.Models;
using CustomAuthentication.ViewModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    public class SalaryController : InventoryBaseController
    {
        [RequirePermission("Salaries", PermissionAction.View)]
        public ActionResult Index()
        {
            var salaries = db.Salaries
                .Include(s => s.User)
                .OrderByDescending(s => s.SalaryYear)
                .ThenByDescending(s => s.SalaryMonth)
                .ToList();

            return View(salaries);
        }

        [RequirePermission("Salaries", PermissionAction.View)]
        public ActionResult Details(int id)
        {
            var salary = db.Salaries
                .Include(s => s.User)
                .FirstOrDefault(s => s.SalaryId == id);

            if (salary == null)
            {
                return HttpNotFound();
            }

            return View(salary);
        }

        [RequirePermission("Salaries", PermissionAction.Create)]
        [HttpGet]
        public ActionResult Create()
        {
            var model = new SalaryViewModel
            {
                SalaryMonth = DateTime.Now.Month,
                SalaryYear = DateTime.Now.Year
            };

            LoadDropdowns(model);

            return View(model);
        }

        [RequirePermission("Salaries", PermissionAction.Create)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SalaryViewModel model)
        {
            bool alreadyExists = db.Salaries.Any(
                s => s.UserId == model.UserId
                  && s.SalaryMonth == model.SalaryMonth
                  && s.SalaryYear == model.SalaryYear);

            if (alreadyExists)
            {
                ModelState.AddModelError("", "A salary record for this user and month already exists.");
            }

            if (!ModelState.IsValid)
            {
                LoadDropdowns(model);
                return View(model);
            }

            var salary = new Salary
            {
                UserId = model.UserId,
                BasicSalary = model.BasicSalary,
                Bonus = model.Bonus,
                Deduction = model.Deduction,
                NetSalary = model.BasicSalary + model.Bonus - model.Deduction,
                SalaryMonth = model.SalaryMonth,
                SalaryYear = model.SalaryYear,
                Status = "Pending",
                CreatedByUserId = CurrentUser?.UserId ?? 0,
                Remarks = model.Remarks,
                IsActive = true
            };

            db.Salaries.Add(salary);
            db.SaveChanges();

            TempData["Success"] = "Salary record created successfully.";
            return RedirectToAction("Index");
        }

        [RequirePermission("Salaries", PermissionAction.Edit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkPaid(int id)
        {
            var salary = db.Salaries.Find(id);

            if (salary == null)
            {
                return HttpNotFound();
            }

            salary.Status = "Paid";
            salary.PaidDate = DateTime.Now;

            db.SaveChanges();

            TempData["Success"] = "Salary marked as paid.";
            return RedirectToAction("Details", new { id });
        }

        [RequirePermission("Salaries", PermissionAction.Delete)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var salary = db.Salaries.Find(id);

            if (salary == null)
            {
                return HttpNotFound();
            }

            if (salary.Status == "Paid")
            {
                TempData["Error"] = "A paid salary record cannot be deleted.";
                return RedirectToAction("Index");
            }

            db.Salaries.Remove(salary);
            db.SaveChanges();

            TempData["Success"] = "Salary record deleted.";
            return RedirectToAction("Index");
        }

        private void LoadDropdowns(SalaryViewModel model)
        {
            model.UserOptions = db.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FullName)
                .Select(u => new SelectListItem { Value = u.UserId.ToString(), Text = u.FullName + " (" + u.RoleName + ")" })
                .ToList();

            model.MonthOptions = new[]
            {
                new SelectListItem { Value = "1", Text = "January" },
                new SelectListItem { Value = "2", Text = "February" },
                new SelectListItem { Value = "3", Text = "March" },
                new SelectListItem { Value = "4", Text = "April" },
                new SelectListItem { Value = "5", Text = "May" },
                new SelectListItem { Value = "6", Text = "June" },
                new SelectListItem { Value = "7", Text = "July" },
                new SelectListItem { Value = "8", Text = "August" },
                new SelectListItem { Value = "9", Text = "September" },
                new SelectListItem { Value = "10", Text = "October" },
                new SelectListItem { Value = "11", Text = "November" },
                new SelectListItem { Value = "12", Text = "December" }
            };
        }
    }
}

using CustomAuthentication.Filters;
using CustomAuthentication.Models;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    public class UnitController : InventoryBaseController
    {
        [RequirePermission("Units", PermissionAction.View)]
        public ActionResult Index()
        {
            var units = db.Units
                .OrderBy(u => u.Name)
                .ToList();

            return View(units);
        }

        [RequirePermission("Units", PermissionAction.Create)]
        [HttpGet]
        public ActionResult Create()
        {
            return View(new Unit { IsActive = true });
        }

        [RequirePermission("Units", PermissionAction.Create)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Unit model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Unit name is required.");
            }

            if (string.IsNullOrWhiteSpace(model.ShortCode))
            {
                ModelState.AddModelError("ShortCode", "Short code is required.");
            }

            if (ModelState.IsValid && db.Units.Any(u => u.Name == model.Name.Trim()))
            {
                ModelState.AddModelError("Name", "A unit with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Name = model.Name.Trim();
            model.ShortCode = model.ShortCode.Trim();

            db.Units.Add(model);
            db.SaveChanges();

            TempData["Success"] = "Unit added successfully.";
            return RedirectToAction("Index");
        }

        [RequirePermission("Units", PermissionAction.Edit)]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var unit = db.Units.Find(id);

            if (unit == null)
            {
                return HttpNotFound();
            }

            return View(unit);
        }

        [RequirePermission("Units", PermissionAction.Edit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Unit model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Unit name is required.");
            }

            if (string.IsNullOrWhiteSpace(model.ShortCode))
            {
                ModelState.AddModelError("ShortCode", "Short code is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var unit = db.Units.Find(model.UnitId);

            if (unit == null)
            {
                return HttpNotFound();
            }

            unit.Name = model.Name.Trim();
            unit.ShortCode = model.ShortCode.Trim();
            unit.IsActive = model.IsActive;

            db.SaveChanges();

            TempData["Success"] = "Unit updated successfully.";
            return RedirectToAction("Index");
        }

        [RequirePermission("Units", PermissionAction.Delete)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var unit = db.Units.Find(id);

            if (unit == null)
            {
                return HttpNotFound();
            }

            bool inUse = db.Products.Any(p => p.UnitId == id);

            if (inUse)
            {
                TempData["Error"] = "This unit is used by one or more products and cannot be deleted. Mark it inactive instead.";
                return RedirectToAction("Index");
            }

            db.Units.Remove(unit);
            db.SaveChanges();

            TempData["Success"] = "Unit deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}

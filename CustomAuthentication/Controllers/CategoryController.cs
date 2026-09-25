using CustomAuthentication.Filters;
using CustomAuthentication.Models;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    public class CategoryController : InventoryBaseController
    {
        // =========================
        // LIST
        // =========================
        [RequirePermission("Categories", PermissionAction.View)]
        public ActionResult Index()
        {
            var categories = db.Categories
                .OrderBy(c => c.Name)
                .ToList();

            return View(categories);
        }

        // =========================
        // CREATE
        // =========================
        [RequirePermission("Categories", PermissionAction.Create)]
        [HttpGet]
        public ActionResult Create()
        {
            return View(new Category { IsActive = true });
        }

        [RequirePermission("Categories", PermissionAction.Create)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Category name is required.");
            }
            else if (db.Categories.Any(c => c.Name == model.Name.Trim()))
            {
                ModelState.AddModelError("Name", "A category with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Name = model.Name.Trim();
            db.Categories.Add(model);
            db.SaveChanges();

            TempData["Success"] = "Category added successfully.";
            return RedirectToAction("Index");
        }

        // =========================
        // EDIT
        // =========================
        [RequirePermission("Categories", PermissionAction.Edit)]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var category = db.Categories.Find(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        [RequirePermission("Categories", PermissionAction.Edit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Category name is required.");
            }
            else if (db.Categories.Any(c => c.Name == model.Name.Trim() && c.CategoryId != model.CategoryId))
            {
                ModelState.AddModelError("Name", "A category with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var category = db.Categories.Find(model.CategoryId);

            if (category == null)
            {
                return HttpNotFound();
            }

            category.Name = model.Name.Trim();
            category.Description = model.Description;
            category.IsActive = model.IsActive;

            db.SaveChanges();

            TempData["Success"] = "Category updated successfully.";
            return RedirectToAction("Index");
        }

        // =========================
        // DELETE
        // =========================
        [RequirePermission("Categories", PermissionAction.Delete)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var category = db.Categories.Find(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            bool inUse = db.Products.Any(p => p.CategoryId == id);

            if (inUse)
            {
                TempData["Error"] = "This category is used by one or more products and cannot be deleted. Mark it inactive instead.";
                return RedirectToAction("Index");
            }

            db.Categories.Remove(category);
            db.SaveChanges();

            TempData["Success"] = "Category deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}

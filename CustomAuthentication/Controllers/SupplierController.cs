using CustomAuthentication.Filters;
using CustomAuthentication.Models;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    public class SupplierController : InventoryBaseController
    {
        [RequirePermission("Suppliers", PermissionAction.View)]
        public ActionResult Index()
        {
            var suppliers = db.Suppliers
                .OrderBy(s => s.Name)
                .ToList();

            return View(suppliers);
        }

        [RequirePermission("Suppliers", PermissionAction.Create)]
        [HttpGet]
        public ActionResult Create()
        {
            return View(new Supplier { IsActive = true });
        }

        [RequirePermission("Suppliers", PermissionAction.Create)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Supplier model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Supplier name is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Name = model.Name.Trim();
            db.Suppliers.Add(model);
            db.SaveChanges();

            TempData["Success"] = "Supplier added successfully.";
            return RedirectToAction("Index");
        }

        [RequirePermission("Suppliers", PermissionAction.Edit)]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var supplier = db.Suppliers.Find(id);

            if (supplier == null)
            {
                return HttpNotFound();
            }

            return View(supplier);
        }

        [RequirePermission("Suppliers", PermissionAction.Edit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Supplier model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Supplier name is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var supplier = db.Suppliers.Find(model.SupplierId);

            if (supplier == null)
            {
                return HttpNotFound();
            }

            supplier.Name = model.Name.Trim();
            supplier.Phone = model.Phone;
            supplier.Email = model.Email;
            supplier.Address = model.Address;
            supplier.IsActive = model.IsActive;

            db.SaveChanges();

            TempData["Success"] = "Supplier updated successfully.";
            return RedirectToAction("Index");
        }

        [RequirePermission("Suppliers", PermissionAction.Delete)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var supplier = db.Suppliers.Find(id);

            if (supplier == null)
            {
                return HttpNotFound();
            }

            bool inUse = db.Purchases.Any(p => p.SupplierId == id);

            if (inUse)
            {
                TempData["Error"] = "This supplier has purchase records and cannot be deleted. Mark it inactive instead.";
                return RedirectToAction("Index");
            }

            db.Suppliers.Remove(supplier);
            db.SaveChanges();

            TempData["Success"] = "Supplier deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}

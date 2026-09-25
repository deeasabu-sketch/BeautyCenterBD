using CustomAuthentication.Filters;
using CustomAuthentication.Models;
using System.Linq;
using System.Web.Mvc;

namespace CustomAuthentication.Controllers
{
    public class WarehouseController : InventoryBaseController
    {
        [RequirePermission("Warehouses", PermissionAction.View)]
        public ActionResult Index()
        {
            var warehouses = db.Warehouses
                .OrderBy(w => w.Name)
                .ToList();

            return View(warehouses);
        }

        [RequirePermission("Warehouses", PermissionAction.Create)]
        [HttpGet]
        public ActionResult Create()
        {
            return View(new Warehouse { IsActive = true });
        }

        [RequirePermission("Warehouses", PermissionAction.Create)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Warehouse model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Warehouse name is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Name = model.Name.Trim();
            db.Warehouses.Add(model);
            db.SaveChanges();

            TempData["Success"] = "Warehouse added successfully.";
            return RedirectToAction("Index");
        }

        [RequirePermission("Warehouses", PermissionAction.Edit)]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var warehouse = db.Warehouses.Find(id);

            if (warehouse == null)
            {
                return HttpNotFound();
            }

            return View(warehouse);
        }

        [RequirePermission("Warehouses", PermissionAction.Edit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Warehouse model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Warehouse name is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var warehouse = db.Warehouses.Find(model.WarehouseId);

            if (warehouse == null)
            {
                return HttpNotFound();
            }

            warehouse.Name = model.Name.Trim();
            warehouse.Code = model.Code;
            warehouse.Address = model.Address;
            warehouse.Phone = model.Phone;
            warehouse.IsActive = model.IsActive;

            db.SaveChanges();

            TempData["Success"] = "Warehouse updated successfully.";
            return RedirectToAction("Index");
        }

        [RequirePermission("Warehouses", PermissionAction.Delete)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var warehouse = db.Warehouses.Find(id);

            if (warehouse == null)
            {
                return HttpNotFound();
            }

            bool inUse = db.Users.Any(u => u.WarehouseId == id)
                || db.Purchases.Any(p => p.WarehouseId == id)
                || db.Sales.Any(s => s.WarehouseId == id)
                || db.Orders.Any(o => o.WarehouseId == id)
                || db.StockTransfers.Any(t => t.FromWarehouseId == id || t.ToWarehouseId == id);

            if (inUse)
            {
                TempData["Error"] = "This warehouse is in use (users, purchases, sales, orders or transfers reference it) and cannot be deleted. Mark it inactive instead.";
                return RedirectToAction("Index");
            }

            db.Warehouses.Remove(warehouse);
            db.SaveChanges();

            TempData["Success"] = "Warehouse deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
